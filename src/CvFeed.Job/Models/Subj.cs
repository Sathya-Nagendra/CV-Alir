using System.Text.Json.Serialization;

namespace CvFeed.Job.Models;

/// <summary>SUBJ - Subject segment. One per subject; today one per transaction (the insured business).</summary>
public sealed class Subj
{
    /// <summary>I or B; "B" today</summary>
    [JsonPropertyName("SubjectType")]
    public string SubjectType { get; init; } = "B";

    /// <summary>Table 7; "PP" (primary policyholder) today</summary>
    [JsonPropertyName("RelationToPolicyHolderCode")]
    public string RelationToPolicyHolderCode { get; init; } = "PP";

    /// <summary>Table 10; blank (drivers not reported)</summary>
    [JsonPropertyName("RelationToInsuredCode")]
    public string RelationToInsuredCode { get; init; } = "";

    [JsonPropertyName("Name")]
    public SubjectName Name { get; init; } = new();

    /// <summary>Key "Id" per the reference file; guide p.41 says "ID"</summary>
    [JsonPropertyName("Id")]
    public SubjectId Id { get; init; } = new();

    /// <summary>Written as an empty object {} for a business</summary>
    [JsonPropertyName("Details")]
    public SubjectDetails Details { get; init; } = new();

    /// <summary>Y, N or blank</summary>
    [JsonPropertyName("OptOutFlag")]
    public string OptOutFlag { get; init; } = "";
}

public sealed class SubjectName
{
    [JsonPropertyName("BusinessName")]
    public string BusinessName { get; init; } = "";
}

public sealed class SubjectId
{
    /// <summary>FEIN; blank until available in IMS</summary>
    [JsonPropertyName("FederalId")]
    public string FederalId { get; init; } = "";

    /// <summary>Policy number; matches VehicleRegistrant.StateID. Key per the reference file; guide says "StateId".</summary>
    [JsonPropertyName("StateID")]
    public string StateID { get; init; } = "";
}

/// <summary>Individual details (DOB, gender, marital status). Empty for business subjects.</summary>
public sealed class SubjectDetails
{
}
