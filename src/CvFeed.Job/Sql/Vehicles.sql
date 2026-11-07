/* Embedded in CvFeed.Job. Parameters: @PeriodBegin date, @PeriodEnd date, @TransactionTypes varchar(100) (NULL = all).
   Source of truth: sql/cv_feed_queries.sql. Keep the two in step. */

WITH scoped AS (
    -- Same transactions and type rules as Query 1
    SELECT q.NetRate_QuoteID,
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
    WHERE q.QuoteStatusID IN (3, 6, 10, 12, 17, 28)
      AND q.NetRate_QuoteID IS NOT NULL
      AND q.DateBound >= @PeriodBegin                     -- reporting period (bind date),
      AND q.DateBound <  DATEADD(DAY, 1, @PeriodEnd)      -- end date inclusive
),
txn AS (
    -- NetRate quotes behind the requested transaction types only
    SELECT DISTINCT NetRate_QuoteID FROM scoped
    WHERE @TransactionTypes IS NULL                          -- NULL = all types
       OR ',' + REPLACE(@TransactionTypes, ' ', '') + ',' LIKE '%,' + CvTransactionType + ',%'
)
SELECT
    /* ---------- Keys --------------------------------------------------------------- */
    loc.QuoteID                 AS NetRate_QuoteID,  -- join to Query 1 (tblQuotes.NetRate_QuoteID)
    loc.LocationID,                                  -- internal key
    b.BusinessAutoID,                                -- internal key
    v.VehiclePolicyLimitsID     AS VehicleRowID,     -- internal key; joins to coverages below

    /* ---------- Vehicle: ASSET.VehicleDetails --------------------------------------- */
    v.VIN,                                           -- VehicleDetails.VIN
    v.Make,                                          -- VehicleDetails.VehicleMake
    v.Model,                                         -- VehicleDetails.VehicleModel
    v.ModelYear,                                     -- VehicleDetails.VehicleYear
    v.VehicleTypeDesc,                               -- VehicleDetails.VehicleTypeCode (via Table 20 crosswalk)
    v.ClassCode,                                     -- VehicleDetails.ISOAutomobileClassCode; crosswalk fallback
    v.FullClassCode,                                 -- candidate ISO class code (QUESTION for Verisk)
    v.RegState,                                      -- VehicleDetails.VehicleRegistrationStateCode
    v.BusinessUse,                                   -- VehicleDetails.BusinessUse (Y/N)
    v.[Use]                     AS UseDesc,          -- not mapped (reference only)

    /* ---------- Garaging location: ASSET.VehicleDetails.GaragingAddress ------------- */
    loc.Address                 AS GarAddress,       -- GaragingAddress.Street1
    loc.City                    AS GarCity,          -- GaragingAddress.City
    loc.State                   AS GarState,         -- GaragingAddress.State; fallback for registration state
    loc.ZipCode                 AS GarZip,           -- GaragingAddress.Zip

    /* ---------- Coverages: ASSET.VehicleDetails.PolicyCoverageLimits ---------------- */
    e.Uninsured,                                     -- UMSL -> CombinedSingleLimit (code QUESTION for Verisk)
    e.Medical,                                       -- MPAY -> IndividualLimit (field QUESTION for Verisk)
    e.Comprehensive,                                 -- COMP coverage row + DeductibleAmount2
    e.Collision,                                     -- COLL coverage row + DeductibleAmount1
    e.PersonalInjury,                                -- PIP: not mapped yet (QUESTION for Verisk)
    e.PIPDeductible,                                 -- PIP: not mapped yet
    e.AdditionalPIP                                  -- PIP: not mapped yet
    -- Liability (CBSL) is policy-level: Query 1, PolicyLiabilityLimit

FROM txn t
JOIN NetRate_Quote_Insur_Quote_Locat loc                 WITH (NOLOCK) ON loc.QuoteID = t.NetRate_QuoteID
JOIN NetRate_Quote_Insur_Quote_Locat_Busin b             WITH (NOLOCK) ON b.LocationID = loc.LocationID
JOIN NetRate_Quote_Insur_Quote_Locat_Busin_Vehic v       WITH (NOLOCK) ON v.BusinessAutoID = b.BusinessAutoID
LEFT JOIN NetRate_Quote_Insur_Quote_Locat_Busin_Vehic_Vehic e WITH (NOLOCK)
       ON e.VehicleID = v.VehiclePolicyLimitsID AND e.NodeName = 'VehicleCoverages'
WHERE v.NodeName = 'Vehicle'                         -- vehicle rows only (table also holds limits/totals rows)
ORDER BY loc.QuoteID, v.VehiclePolicyLimitsID
OPTION (RECOMPILE);
