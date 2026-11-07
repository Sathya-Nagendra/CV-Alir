using System.Text.Json;
using System.Text.Json.Nodes;
using CvFeed.Job.Models;
using Xunit;

namespace CvFeed.Tests;

public class CvModelTests
{
    private static readonly string FixturePath =
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "cvgver-sample-small.json");

    /// <summary>
    /// Reading the reference layout into the models and writing it back must give the same
    /// keys, in the same order, with the same values. Proves the models match the sample file.
    /// </summary>
    [Fact]
    public void Models_round_trip_the_reference_layout_exactly()
    {
        var original = File.ReadAllText(FixturePath);

        var file = CvJson.Deserialize(original);
        var written = CvJson.Serialize(file);

        Assert.Equal(Compact(original), Compact(written));
    }

    [Fact]
    public void Business_subject_details_are_written_as_an_empty_object()
    {
        var file = new CvFile
        {
            Records = new CvRecords { Policies = [new CvPolicy { Subjects = [new Subj()] }] },
        };

        var json = JsonNode.Parse(CvJson.Serialize(file))!;
        var details = json["RECORDS"]!["POLICY"]![0]!["SUBJ"]![0]!["Details"]!.AsObject();

        Assert.Empty(details);
    }

    [Fact]
    public void Empty_values_are_written_as_empty_strings_not_null()
    {
        var json = CvJson.Serialize(new CvFile
        {
            Records = new CvRecords { Policies = [new CvPolicy { Assets = [new Asset()], Subjects = [new Subj()] }] },
        });

        Assert.DoesNotContain("null", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Ampersands_are_not_escaped()
    {
        var file = new CvFile
        {
            Records = new CvRecords
            {
                Policies = [new CvPolicy { Polr = new Polr { ProducerAgentInfo = new ProducerAgentInfo { ProducerName = "Robertson Ryan & Associates" } } }],
            },
        };

        Assert.Contains("Robertson Ryan & Associates", CvJson.Serialize(file), StringComparison.Ordinal);
    }

    private static string Compact(string json) =>
        JsonNode.Parse(json)!.ToJsonString(new JsonSerializerOptions { Encoder = CvJson.Options.Encoder });
}
