using System.Text.Json.Serialization;

namespace CvFeed.Job.Models;

/// <summary>
/// Root of the CVGVER JSON file sent to Verisk Coverage Verifier.
/// Shape and key order follow the reference file CVGVER_20250630_0101.json.
/// Every value in the file is a string: dates are yyyyMMdd, numbers are zero-filled
/// to a fixed width, and empty values are "" (never null).
/// </summary>
public sealed class CvFile
{
    [JsonPropertyName("RECORDS")]
    public CvRecords Records { get; init; } = new();
}

public sealed class CvRecords
{
    [JsonPropertyName("EFHR")]
    public Efhr Header { get; init; } = new();

    /// <summary>One entry per transaction (one IMS policy version), in processing order.</summary>
    [JsonPropertyName("POLICY")]
    public List<CvPolicy> Policies { get; init; } = [];
}

/// <summary>One CV transaction: exactly one POLR, one ASSET per vehicle, one SUBJ per subject.</summary>
public sealed class CvPolicy
{
    [JsonPropertyName("POLR")]
    public Polr Polr { get; init; } = new();

    [JsonPropertyName("ASSET")]
    public List<Asset> Assets { get; init; } = [];

    [JsonPropertyName("SUBJ")]
    public List<Subj> Subjects { get; init; } = [];
}
