/* Embedded in CvFeed.Job. Parameters: @PeriodBegin date, @PeriodEnd date, @TransactionTypes varchar(100) (NULL = all).
   Source of truth: sql/cv_feed_queries.sql. Keep the two in step. */

WITH scoped AS (
    -- The bound versions in the reporting period, with their CV transaction type
    -- (same rules as Get-CvTransactionType in Export-CvFeed.ps1)
    SELECT q.QuoteID, q.QuoteGuid, q.RenewalOfQuoteGuid, q.EffectiveDate,
        CASE
            WHEN q.QuoteStatusID = 6  THEN 'CN'                        -- notice of intent to cancel
            WHEN q.QuoteStatusID = 17 THEN 'CR'                        -- notice of intent to non-renew
            WHEN q.QuoteStatusID = 28 THEN 'RC'                        -- rescission of a notice
            WHEN LTRIM(RTRIM(q.TransactionTypeID)) = 'C'
                 AND q.QuoteStatusID = 12 THEN 'CC'                    -- cancellation
            WHEN LTRIM(RTRIM(q.TransactionTypeID)) = 'R' THEN 'RI'     -- reinstatement
            WHEN LTRIM(RTRIM(q.TransactionTypeID)) IN ('E','A','N','U','D') THEN 'ED'  -- endorsement
            WHEN q.PolicyTypeID IN (1, 3) THEN 'NB'                    -- new business / rewrite
            WHEN q.PolicyTypeID = 2
                 AND pr.ExpirationDate IS NOT NULL AND q.EffectiveDate IS NOT NULL
                 AND CAST(q.EffectiveDate AS date) <> CAST(pr.ExpirationDate AS date)
                 THEN 'WL'                                             -- renewal with a gap
            WHEN q.PolicyTypeID = 2 THEN 'RW'                          -- renewal
            ELSE 'NB'                                                  -- unmapped PolicyTypeID (as script)
        END AS CvTransactionType
    FROM tblQuotes q WITH (NOLOCK)
    JOIN lstLines l ON l.LineGUID = q.LineGUID AND l.LineID IN (72, 74)
    LEFT JOIN tblQuotes pr WITH (NOLOCK) ON pr.QuoteGuid = q.RenewalOfQuoteGuid
    WHERE q.QuoteStatusID IN (3, 6, 10, 12, 17, 28)          -- bound statuses
      AND q.DateBound >= @PeriodBegin                     -- reporting period (bind date),
      AND q.DateBound <  DATEADD(DAY, 1, @PeriodEnd)      -- end date inclusive
),
txn AS (
    -- Keep only the requested transaction types
    SELECT * FROM scoped
    WHERE @TransactionTypes IS NULL                          -- NULL = all types
       OR ',' + REPLACE(@TransactionTypes, ' ', '') + ',' LIKE '%,' + CvTransactionType + ',%'
),
lineage AS (
    -- Walk back through prior terms (RenewalOfQuoteGuid), same lines only, max 25 terms
    SELECT t.QuoteID AS StartQuoteID, t.QuoteGuid, t.RenewalOfQuoteGuid, t.EffectiveDate, 0 AS Depth
    FROM txn t
    UNION ALL
    SELECT ln.StartQuoteID, p.QuoteGuid, p.RenewalOfQuoteGuid, p.EffectiveDate, ln.Depth + 1
    FROM lineage ln
    JOIN tblQuotes p WITH (NOLOCK) ON p.QuoteGuid = ln.RenewalOfQuoteGuid
    JOIN lstLines pl ON pl.LineGUID = p.LineGUID AND pl.LineID IN (72, 74)
    WHERE ln.Depth < 25
),
inception AS (
    -- Earliest term found = policy inception
    SELECT StartQuoteID, EffectiveDate AS InceptionDate
    FROM (SELECT StartQuoteID, EffectiveDate,
                 ROW_NUMBER() OVER (PARTITION BY StartQuoteID ORDER BY Depth DESC) AS rn
          FROM lineage
          WHERE EffectiveDate IS NOT NULL) x
    WHERE rn = 1
)
SELECT
    /* ---------- Keys and traceability ---------------------------------------------- */
    q.QuoteID,                                   -- POLR.ClientTransactionID
    q.QuoteGuid,                                 -- internal key (lineage)
    q.ControlNo,                                 -- policy chain (all versions of one policy)
    q.NetRate_QuoteID,                           -- join key for 02_vehicles.sql (ASSET)
    l.LineID,                                    -- 72 / 74; both report POLR.LineOfBusiness = CA

    /* ---------- Transaction type (POLR.TransactionType) and its inputs ---------- */
    t.CvTransactionType,                         -- POLR.TransactionType: NB, ED, CC, CN, CR, RC, RI, RW, WL
    q.QuoteStatusID,                             -- 6 CN, 17 CR, 28 RC, 12 + C = CC
    q.TransactionTypeID,                         -- C cancel, R reinstate, E/A/N/U/D endorsement
    q.PolicyTypeID,                              -- 1 new, 2 renewal, 3 rewrite
    q.QuoteStatusReasonID,                       -- raw reason ID
    cancr.Reason                AS StatusReason, -- cancellation reason (future: NP/FC/IC codes)
    q.EndorsementNum,                            -- present = endorsement-type version

    /* ---------- Policy identity ---------------------------------------------------- */
    q.PolicyNumber,                              -- POLR.PolicyNumber; also ASSET.VehicleRegistrant.StateID
                                                 --   and SUBJ.ID.StateId (registrant link)
    q.ExpiringPolicyNumber,                      -- POLR.AlternatePolicyNumber today (QUESTION for Verisk)
    prior.PolicyNumber          AS PriorTermPolicyNumber,  -- candidate ASSET.PreviousPolicyNumber (rewrites)
    q.StateID,                                   -- IMS policy state (selected by the script; not mapped yet)

    /* ---------- Dates -------------------------------------------------------------- */
    inc.InceptionDate,                           -- POLR.PolicyInceptionDate
    q.EffectiveDate,                             -- POLR.TermBeginDate
    q.ExpirationDate,                            -- POLR.TermExpirationDate
    q.EndorsementEffective,                      -- POLR.EffectiveDateOfChange (endorsement-type versions)
                                                 --   and POLR.CancellationDate (CC, CN, CR)
    q.DateBound,                                 -- POLR.TransactionProcessingDate; period filter; EFHR period
    prior.EffectiveDate         AS PriorTermEffective,
    prior.ExpirationDate        AS PriorTermExpiration,   -- renewal: RW if new term starts here, else WL

    /* ---------- Carrier ------------------------------------------------------------ */
    co.CompanyName              AS CarrierName,  -- POLR.CarrierName; key for pseudo AM Best lookup
    co.NAIC                     AS ImsNaic,      -- POLR.NaicCode (config table is the fallback)
    pco.NAIC                    AS PriorTermNaic,-- candidate POLR.PriorNaicCode (ALIR, QUESTION for Verisk)

    /* ---------- Premium and billing ------------------------------------------------ */
    prem.TotalPremium,                           -- POLR.PolicyPremiumAmount (running full-term total)
    q.BillingTypeID,                             -- candidate PremiumPayPlanCode / MethodOfPayCode (not mapped)

    /* ---------- Insured: POLR.PolicyAddress / PolicyTelephones, SUBJ, ASSET.RegistrationAddress */
    q.InsuredCorporationName,                    -- SUBJ.Name.BusinessName (first choice)
    q.InsuredPolicyName,                         -- SUBJ.Name.BusinessName (fallback)
    q.InsuredAddress1,                           -- PolicyAddress.Street1
    q.InsuredAddress2,                           -- PolicyAddress.Street2
    q.InsuredCity,                               -- PolicyAddress.City
    q.InsuredState,                              -- PolicyAddress.State
    q.InsuredZipCode,                            -- PolicyAddress.Zip (first 5)
    q.InsuredZipPlus,                            -- PolicyAddress.Zip (+4)
    q.InsuredISOCountryCode,                     -- PolicyAddress.Country (script hard-codes US)
    q.InsuredPhone,                              -- PolicyTelephones.AreaCode / Number
    -- FEIN: not available in IMS today (SUBJ.ID.FederalId, VehicleRegistrant.FederalID)

    /* ---------- Producer: POLR.ProducerAgentInfo ----------------------------------- */
    q.ProducerName,                              -- ProducerName (part 1)
    q.ProducerLocationName,                      -- ProducerName (part 2)
    q.ProducerLocationID,                        -- ProducerNumber
    q.ProducerAddress1,                          -- ProducerAddress.Street1
    q.ProducerAddress2,                          -- ProducerAddress.Street2
    q.ProducerCity,                              -- ProducerAddress.City
    q.ProducerState,                             -- ProducerAddress.State
    q.ProducerZipCode,                           -- ProducerAddress.Zip
    q.ProducerPhone,                             -- ProducerPhone.AreaCode / Number

    /* ---------- Policy-level coverage --------------------------------------------- */
    nbl.Liability               AS PolicyLiabilityLimit   -- ASSET PolicyCoverageLimits CBSL (every vehicle)

FROM txn t
JOIN tblQuotes q              WITH (NOLOCK) ON q.QuoteID = t.QuoteID
JOIN lstLines l                             ON l.LineGUID = q.LineGUID
JOIN tblCompanyLines cl       WITH (NOLOCK) ON cl.CompanyLineGUID = q.CompanyLineGuid
JOIN tblCompanyLocations cloc WITH (NOLOCK) ON cloc.CompanyLocationGUID = cl.CompanyLocationGUID
JOIN tblCompanies co          WITH (NOLOCK) ON co.CompanyGUID = cloc.CompanyGUID
LEFT JOIN inception inc                     ON inc.StartQuoteID = q.QuoteID
LEFT JOIN tblQuotes prior     WITH (NOLOCK) ON prior.QuoteGuid = q.RenewalOfQuoteGuid
LEFT JOIN tblCompanyLines pcl WITH (NOLOCK) ON pcl.CompanyLineGUID = prior.CompanyLineGuid
LEFT JOIN tblCompanyLocations pcloc WITH (NOLOCK) ON pcloc.CompanyLocationGUID = pcl.CompanyLocationGUID
LEFT JOIN tblCompanies pco    WITH (NOLOCK) ON pco.CompanyGUID = pcloc.CompanyGUID
LEFT JOIN lstQuoteStatusReasons cancr       ON cancr.ID = q.QuoteStatusReasonID
OUTER APPLY (
    -- Running full-term premium: IMS stores each version's change (base +full,
    -- endorsement +/- change, cancellation -full), so sum all bound versions to date.
    SELECT SUM(p.Premium) AS TotalPremium
    FROM tblQuotes q2 WITH (NOLOCK)
    JOIN tblQuoteOptions o WITH (NOLOCK)        ON o.QuoteGuid = q2.QuoteGuid
    JOIN tblQuoteOptionPremiums p WITH (NOLOCK) ON p.QuoteOptionGuid = o.QuoteOptionGuid
    WHERE q2.ControlNo = q.ControlNo
      AND q2.QuoteID <= q.QuoteID
      AND q2.QuoteStatusID IN (3, 6, 10, 12, 17, 28)
) prem
OUTER APPLY (
    -- Policy-level auto liability limit (one row per NetRate quote expected)
    SELECT TOP 1 nb.Liability
    FROM NetRate_Quote_Insur_Quote_Busin nb WITH (NOLOCK)
    WHERE nb.QuoteID = q.NetRate_QuoteID
) nbl
ORDER BY q.DateBound, q.QuoteID                  -- processing order (CV guide p.22)
OPTION (RECOMPILE, MAXRECURSION 30);
