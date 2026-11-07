using System.Text.Json.Serialization;

namespace CvFeed.Job.Models;

/// <summary>ASSET - one per vehicle on the transaction.</summary>
public sealed class Asset
{
    /// <summary>"V" for vehicle</summary>
    [JsonPropertyName("AssetType")]
    public string AssetType { get; init; } = "V";

    [JsonPropertyName("VehicleDetails")]
    public VehicleDetails VehicleDetails { get; init; } = new();
}

public sealed class VehicleDetails
{
    [JsonPropertyName("VIN")]
    public string VIN { get; init; } = "";

    [JsonPropertyName("VehicleMake")]
    public string VehicleMake { get; init; } = "";

    [JsonPropertyName("VehicleModel")]
    public string VehicleModel { get; init; } = "";

    /// <summary>4 digits</summary>
    [JsonPropertyName("VehicleYear")]
    public string VehicleYear { get; init; } = "";

    /// <summary>Table 20; "800" until the crosswalk is filled</summary>
    [JsonPropertyName("VehicleTypeCode")]
    public string VehicleTypeCode { get; init; } = "800";

    /// <summary>Table 18, 2 letters</summary>
    [JsonPropertyName("VehicleRegistrationStateCode")]
    public string VehicleRegistrationStateCode { get; init; } = "";

    /// <summary>yyyyMMdd; "00000000" today</summary>
    [JsonPropertyName("VehicleInceptionDate")]
    public string VehicleInceptionDate { get; init; } = "00000000";

    /// <summary>yyyyMMdd; "00000000" today</summary>
    [JsonPropertyName("VehicleCertificationDate")]
    public string VehicleCertificationDate { get; init; } = "00000000";

    /// <summary>Y or N</summary>
    [JsonPropertyName("BusinessUse")]
    public string BusinessUse { get; init; } = "N";

    /// <summary>yyyyMMdd; "00000000" today</summary>
    [JsonPropertyName("ReplacedVehicleOriginalEffectiveDate")]
    public string ReplacedVehicleOriginalEffectiveDate { get; init; } = "00000000";

    /// <summary>4 digits; "0000" when not a fleet policy</summary>
    [JsonPropertyName("NumberInFleet")]
    public string NumberInFleet { get; init; } = "0000";

    /// <summary>3 characters; "000" today</summary>
    [JsonPropertyName("GarageTownId")]
    public string GarageTownId { get; init; } = "000";

    /// <summary>3 characters; "000" today</summary>
    [JsonPropertyName("PremiumTownId")]
    public string PremiumTownId { get; init; } = "000";

    /// <summary>16 characters; zero-filled today</summary>
    [JsonPropertyName("BinderNumber")]
    public string BinderNumber { get; init; } = "0000000000000000";

    /// <summary>5 digits; "00000" today</summary>
    [JsonPropertyName("UnpaidPremiumAmount")]
    public string UnpaidPremiumAmount { get; init; } = "00000";

    /// <summary>Y or N; "N" today</summary>
    [JsonPropertyName("OutOfStateInsuranceIndicator")]
    public string OutOfStateInsuranceIndicator { get; init; } = "N";

    /// <summary>Y or N (non-US VIN); "N" today</summary>
    [JsonPropertyName("GrayFlag")]
    public string GrayFlag { get; init; } = "N";

    /// <summary>Table 12; blank today</summary>
    [JsonPropertyName("AntiTheftDeviceCode")]
    public string AntiTheftDeviceCode { get; init; } = "";

    /// <summary>Y, N or blank</summary>
    [JsonPropertyName("LeasedVehicleIndicator")]
    public string LeasedVehicleIndicator { get; init; } = "";

    [JsonPropertyName("PreviousPolicyNumber")]
    public string PreviousPolicyNumber { get; init; } = "";

    [JsonPropertyName("InvalidVinIndicator")]
    public string InvalidVinIndicator { get; init; } = "";

    /// <summary>Collision deductible, zero-filled to 8 (guide p.30 says 5; open with Verisk)</summary>
    [JsonPropertyName("DeductibleAmount1")]
    public string DeductibleAmount1 { get; init; } = "00000000";

    /// <summary>Comprehensive deductible, zero-filled to 8</summary>
    [JsonPropertyName("DeductibleAmount2")]
    public string DeductibleAmount2 { get; init; } = "00000000";

    /// <summary>Zero-filled to 6</summary>
    [JsonPropertyName("ISOAutomobileClassCode")]
    public string ISOAutomobileClassCode { get; init; } = "000000";

    [JsonPropertyName("VehicleRegistrant")]
    public VehicleRegistrant VehicleRegistrant { get; init; } = new();

    [JsonPropertyName("GaragingAddress")]
    public GaragingAddress GaragingAddress { get; init; } = new();

    /// <summary>Insured mailing address (same values as POLR.PolicyAddress)</summary>
    [JsonPropertyName("RegistrationAddress")]
    public Address RegistrationAddress { get; init; } = new();

    /// <summary>Elected coverages only, in order: CBSL, UMSL, MPAY, COMP, COLL</summary>
    [JsonPropertyName("PolicyCoverageLimits")]
    public List<CoverageLimit> PolicyCoverageLimits { get; init; } = [];
}

public sealed class VehicleRegistrant
{
    /// <summary>I or B; "B" today</summary>
    [JsonPropertyName("EntityType")]
    public string EntityType { get; init; } = "B";

    /// <summary>FEIN; blank until available in IMS</summary>
    [JsonPropertyName("FederalID")]
    public string FederalID { get; init; } = "";

    /// <summary>Policy number; links the vehicle to its SUBJ</summary>
    [JsonPropertyName("StateID")]
    public string StateID { get; init; } = "";
}

/// <summary>Garaging address. No Street2 key in the reference file.</summary>
public sealed class GaragingAddress
{
    [JsonPropertyName("Street1")]
    public string Street1 { get; init; } = "";

    [JsonPropertyName("City")]
    public string City { get; init; } = "";

    [JsonPropertyName("State")]
    public string State { get; init; } = "";

    [JsonPropertyName("Zip")]
    public string Zip { get; init; } = "";

    [JsonPropertyName("Country")]
    public string Country { get; init; } = "US";
}

/// <summary>One coverage on a vehicle. Limits are whole dollars zero-filled to 8.</summary>
public sealed class CoverageLimit
{
    /// <summary>Table 6A: CBSL, UMSL, MPAY, COMP, COLL</summary>
    [JsonPropertyName("CoverageTypeCode")]
    public string CoverageTypeCode { get; init; } = "";

    /// <summary>Used by MPAY</summary>
    [JsonPropertyName("IndividualLimit")]
    public string IndividualLimit { get; init; } = "00000000";

    [JsonPropertyName("OccurrenceLimit")]
    public string OccurrenceLimit { get; init; } = "00000000";

    /// <summary>Used by CBSL and UMSL</summary>
    [JsonPropertyName("CombinedSingleLimit")]
    public string CombinedSingleLimit { get; init; } = "00000000";
}
