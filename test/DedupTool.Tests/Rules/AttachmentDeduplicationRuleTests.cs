using DedupTool.Rules;
using Xunit;

namespace DedupTool.Tests.Rules;

public class AttachmentDeduplicationRuleTests
{
    private readonly IDeduplicationRule _rule = new AttachmentDeduplicationRule();

    private static Document Loose(string id, string hash) => new()
    {
        DocId = id, Custodian = "c1", DocType = "loose", FileName = id + ".txt", Sha256 = hash, SizeBytes = 10,
    };

    private static Document Email(string id, string hash) => new()
    {
        DocId = id, Custodian = "c1", DocType = "email", FileName = id + ".eml", Sha256 = hash, SizeBytes = 10,
    };

    private static Document Attachment(string id, string parent, string hash, string custodian = "c1") => new()
    {
        DocId = id, Custodian = custodian, DocType = "attachment", ParentId = parent, FileName = id + ".pdf", Sha256 = hash, SizeBytes = 10,
    };

    private static string[] Ids(IReadOnlyList<Document> docs) => docs.Select(d => d.DocId).ToArray();

    [Fact]
    public void IdenticalAttachments_OnDifferentEmails_OneKept()
    {
        var result = _rule.Run(new[] { Email("E1", "e1"), Email("E2", "e2"), Attachment("T1", "E1", "h1"), Attachment("T2", "E2", "h1") });

        Assert.Equal(new[] { "E1", "E2", "T1" }, Ids(result));
    }

    [Fact]
    public void IdenticalAttachments_InSameEmail_OneKept()
    {
        var result = _rule.Run(new[] { Email("E1", "e1"), Attachment("T1", "E1", "h1"), Attachment("T2", "E1", "h1") });

        Assert.Equal(new[] { "E1", "T1" }, Ids(result));
    }

    [Fact]
    public void Survivor_IsLowestDocId_RegardlessOfInputOrder()
    {
        var result = _rule.Run(new[] { Attachment("T2", "E1", "h1"), Attachment("T1", "E2", "h1") });

        Assert.Equal(new[] { "T1" }, Ids(result));
    }

    [Fact]
    public void ThreeIdenticalAttachments_OneKept()
    {
        var result = _rule.Run(new[] { Attachment("T3", "E1", "h1"), Attachment("T1", "E1", "h1"), Attachment("T2", "E1", "h1") });

        Assert.Equal(new[] { "T1" }, Ids(result));
    }

    [Fact]
    public void HashCaseDifferences_AreStillDuplicates()
    {
        var result = _rule.Run(new[] { Attachment("T1", "E1", "ABCDEF"), Attachment("T2", "E1", "abcdef") });

        Assert.Equal(new[] { "T1" }, Ids(result));
    }

    [Fact]
    public void DuplicatesAcrossCustodians_AreSuppressed()
    {
        var result = _rule.Run(new[] { Attachment("T1", "E1", "h1", "c1"), Attachment("T2", "E2", "h1", "c2") });

        Assert.Equal(new[] { "T1" }, Ids(result));
    }

    [Fact]
    public void Attachment_WithSameHashAsLooseFileOrEmail_IsNotSuppressed()
    {
        var docs = new[] { Email("E1", "h1"), Loose("L1", "h2"), Attachment("T1", "E1", "h1"), Attachment("T2", "E1", "h2") };

        var result = _rule.Run(docs);

        Assert.Equal(new[] { "E1", "L1", "T1", "T2" }, Ids(result));
    }

    [Fact]
    public void RemainingDocuments_KeepInputOrder()
    {
        var result = _rule.Run(new[] { Attachment("Z", "E1", "h3"), Attachment("B", "E1", "h1"), Attachment("A", "E1", "h1"), Attachment("M", "E1", "h2") });

        Assert.Equal(new[] { "Z", "A", "M" }, Ids(result));
    }

    [Fact]
    public void EmptyInput_ReturnsEmpty()
    {
        Assert.Empty(_rule.Run(Array.Empty<Document>()));
    }
}
