using System.Text.Json.Serialization;

namespace CvFeed.Job.Models;

/// <summary>EFHR - Electronic File Header. One per file.</summary>
public sealed class Efhr
{
    /// <summary>"CF01" per the reference file (guide p.25 says "1.0"; open with Verisk).</summary>
    [JsonPropertyName("Version")]
    public string Version { get; init; } = "CF01";

    /// <summary>yyyyMMdd</summary>
    [JsonPropertyName("DateCreated")]
    public string DateCreated { get; init; } = "";

    /// <summary>HHmmss, 24-hour clock</summary>
    [JsonPropertyName("TimeCreated")]
    public string TimeCreated { get; init; } = "";

    /// <summary>yyyyMMdd; also the date in the file name</summary>
    [JsonPropertyName("ReportingPeriodBeginDate")]
    public string ReportingPeriodBeginDate { get; init; } = "";

    /// <summary>yyyyMMdd</summary>
    [JsonPropertyName("ReportingPeriodEndDate")]
    public string ReportingPeriodEndDate { get; init; } = "";
}
