using System.Text.Encodings.Web;
using System.Text.Json;

namespace CvFeed.Job.Models;

/// <summary>Serializer settings that reproduce the reference file's layout.</summary>
public static class CvJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        // Keep characters like & and ' as-is (e.g. "Robertson Ryan & Associates") instead of &.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Serialize(CvFile file) => JsonSerializer.Serialize(file, Options);

    public static CvFile Deserialize(string json) =>
        JsonSerializer.Deserialize<CvFile>(json, Options)
        ?? throw new JsonException("The JSON did not contain a CVGVER file.");
}
