using System.Text.Json.Serialization;

namespace CvFeed.Job.Models;

/// <summary>POLR - Policy segment. Exactly one per transaction.</summary>
public sealed class Polr
{
    [JsonPropertyName("PolicyNumber")]
    public string PolicyNumber { get; init; } = "";

    [JsonPropertyName("AlternatePolicyNumber")]
    public string AlternatePolicyNumber { get; init; } = "";

    [JsonPropertyName("CarrierName")]
    public string CarrierName { get; init; } = "";

    /// <summary>Pseudo AM Best number (5 digits). Key name per the reference file; guide p.26 says "AMBestNumber".</summary>
    [JsonPropertyName("IssuerAMBestCompanyNumber")]
    public string IssuerAMBestCompanyNumber { get; init; } = "";

    /// <summary>5 digits</summary>
    [JsonPropertyName("NaicCode")]
    public string NaicCode { get; init; } = "";

    /// <summary>Table 2; always "CA"</summary>
    [JsonPropertyName("LineOfBusiness")]
    public string LineOfBusiness { get; init; } = "CA";

    /// <summary>Table 1; blank today</summary>
    [JsonPropertyName("RiskClassCode")]
    public string RiskClassCode { get; init; } = "";

    /// <summary>Table 3; "AU" today</summary>
    [JsonPropertyName("PolicyClassCode")]
    public string PolicyClassCode { get; init; } = "AU";

    /// <summary>yyyyMMdd</summary>
    [JsonPropertyName("PolicyInceptionDate")]
    public string PolicyInceptionDate { get; init; } = "";

    /// <summary>yyyyMMdd</summary>
    [JsonPropertyName("TermBeginDate")]
    public string TermBeginDate { get; init; } = "";

    /// <summary>yyyyMMdd</summary>
    [JsonPropertyName("TermExpirationDate")]
    public string TermExpirationDate { get; init; } = "";

    /// <summary>yyyyMMdd (transaction effective date)</summary>
    [JsonPropertyName("EffectiveDateOfChange")]
    public string EffectiveDateOfChange { get; init; } = "";

    /// <summary>yyyyMMdd; "00000000" when not a cancellation</summary>
    [JsonPropertyName("CancellationDate")]
    public string CancellationDate { get; init; } = "00000000";

    /// <summary>Table 5: NB, ED, CC, CN, CR, RC, RI, RW, WL</summary>
    [JsonPropertyName("TransactionType")]
    public string TransactionType { get; init; } = "";

    /// <summary>Table 13; blank for commercial auto</summary>
    [JsonPropertyName("ISOPolicyFormNumber")]
    public string ISOPolicyFormNumber { get; init; } = "";

    /// <summary>Insured mailing address</summary>
    [JsonPropertyName("PolicyAddress")]
    public Address PolicyAddress { get; init; } = new();

    [JsonPropertyName("PolicyTelephones")]
    public List<PolicyTelephone> PolicyTelephones { get; init; } = [];

    /// <summary>Dollars and cents, no decimal point, zero-filled to 9 (e.g. "024961100" = $249,611.00)</summary>
    [JsonPropertyName("PolicyPremiumAmount")]
    public string PolicyPremiumAmount { get; init; } = "000000000";

    /// <summary>Table 4; blank today</summary>
    [JsonPropertyName("PremiumPayPlanCode")]
    public string PremiumPayPlanCode { get; init; } = "";

    /// <summary>Table 8; "OT" today</summary>
    [JsonPropertyName("MethodOfPayCode")]
    public string MethodOfPayCode { get; init; } = "OT";

    /// <summary>yyyyMMdd (bind date)</summary>
    [JsonPropertyName("TransactionProcessingDate")]
    public string TransactionProcessingDate { get; init; } = "";

    [JsonPropertyName("ProducerAgentInfo")]
    public ProducerAgentInfo ProducerAgentInfo { get; init; } = new();

    /// <summary>IMS QuoteID; up to 15 characters</summary>
    [JsonPropertyName("ClientTransactionID")]
    public string ClientTransactionID { get; init; } = "";

    /// <summary>Up to 11 characters; blank today</summary>
    [JsonPropertyName("GroupId")]
    public string GroupId { get; init; } = "";

    /// <summary>Y, N or blank (Exchange only)</summary>
    [JsonPropertyName("EffectiveUntilCancelledIndicator")]
    public string EffectiveUntilCancelledIndicator { get; init; } = "";

    /// <summary>5 digits (ALIR only); blank today</summary>
    [JsonPropertyName("PriorNaicCode")]
    public string PriorNaicCode { get; init; } = "";

    /// <summary>Y, N or blank; "N" today</summary>
    [JsonPropertyName("LightspeedFlag")]
    public string LightspeedFlag { get; init; } = "N";

    [JsonPropertyName("LightspeedIDNumber")]
    public string LightspeedIDNumber { get; init; } = "";
}

/// <summary>Six-line address used for PolicyAddress, ProducerAddress and RegistrationAddress.</summary>
public sealed class Address
{
    [JsonPropertyName("Street1")]
    public string Street1 { get; init; } = "";

    [JsonPropertyName("Street2")]
    public string Street2 { get; init; } = "";

    [JsonPropertyName("City")]
    public string City { get; init; } = "";

    /// <summary>Table 18, 2 letters</summary>
    [JsonPropertyName("State")]
    public string State { get; init; } = "";

    /// <summary>5 or 9 digits</summary>
    [JsonPropertyName("Zip")]
    public string Zip { get; init; } = "";

    /// <summary>Table 11; "US"</summary>
    [JsonPropertyName("Country")]
    public string Country { get; init; } = "US";
}

public sealed class PolicyTelephone
{
    /// <summary>"B" (business) today</summary>
    [JsonPropertyName("Type")]
    public string Type { get; init; } = "B";

    /// <summary>3 digits or blank</summary>
    [JsonPropertyName("AreaCode")]
    public string AreaCode { get; init; } = "";

    /// <summary>7 digits or blank</summary>
    [JsonPropertyName("Number")]
    public string Number { get; init; } = "";
}

public sealed class ProducerAgentInfo
{
    [JsonPropertyName("ProducerName")]
    public string ProducerName { get; init; } = "";

    [JsonPropertyName("ProducerNumber")]
    public string ProducerNumber { get; init; } = "";

    [JsonPropertyName("ProducerEmail")]
    public string ProducerEmail { get; init; } = "";

    [JsonPropertyName("ProducerAddress")]
    public Address ProducerAddress { get; init; } = new();

    [JsonPropertyName("ProducerPhone")]
    public Phone ProducerPhone { get; init; } = new();
}

public sealed class Phone
{
    /// <summary>3 digits or blank</summary>
    [JsonPropertyName("AreaCode")]
    public string AreaCode { get; init; } = "";

    /// <summary>7 digits or blank</summary>
    [JsonPropertyName("Number")]
    public string Number { get; init; } = "";
}
