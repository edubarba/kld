using DedupTool.Rules;
using Xunit;

namespace DedupTool.Tests.Rules;

public class LooseFileDeduplicationRuleTests
{
    private readonly IDeduplicationRule _rule = new LooseFileDeduplicationRule();

    private static Document Loose(string id, string hash, string custodian = "c1") => new()
    {
        DocId = id, Custodian = custodian, DocType = "loose", FileName = id + ".txt", Sha256 = hash, SizeBytes = 10,
    };

    private static Document Email(string id, string hash) => new()
    {
        DocId = id, Custodian = "c1", DocType = "email", FileName = id + ".eml", Sha256 = hash, SizeBytes = 10,
    };

    private static Document Attachment(string id, string parent, string hash) => new()
    {
        DocId = id, Custodian = "c1", DocType = "attachment", ParentId = parent, FileName = id + ".pdf", Sha256 = hash, SizeBytes = 10,
    };

    private static string[] Ids(IReadOnlyList<Document> docs) => docs.Select(d => d.DocId).ToArray();

    [Fact]
    public void IdenticalLooseFiles_OneKept()
    {
        var result = _rule.Run(new[] { Loose("A", "h1"), Loose("B", "h1") });

        Assert.Equal(new[] { "A" }, Ids(result));
    }

    [Fact]
    public void Survivor_IsLowestDocId_RegardlessOfInputOrder()
    {
        var result = _rule.Run(new[] { Loose("B", "h1"), Loose("A", "h1") });

        Assert.Equal(new[] { "A" }, Ids(result));
    }

    [Fact]
    public void HashCaseDifferences_AreStillDuplicates()
    {
        var result = _rule.Run(new[] { Loose("A", "ABCDEF"), Loose("B", "abcdef") });

        Assert.Equal(new[] { "A" }, Ids(result));
    }

    [Fact]
    public void DuplicatesAcrossCustodians_AreSuppressed()
    {
        var result = _rule.Run(new[] { Loose("A", "h1", "c1"), Loose("B", "h1", "c2") });

        Assert.Equal(new[] { "A" }, Ids(result));
    }

    [Fact]
    public void LooseFile_WithSameHashAsEmailOrAttachment_IsNotSuppressed()
    {
        var docs = new[] { Email("E1", "h1"), Attachment("T1", "E1", "h2"), Loose("L1", "h1"), Loose("L2", "h2") };

        var result = _rule.Run(docs);

        Assert.Equal(new[] { "E1", "T1", "L1", "L2" }, Ids(result));
    }

    [Fact]
    public void EmailsAndAttachments_WithSameHash_PassThroughUntouched()
    {
        var docs = new[] { Email("E1", "h1"), Email("E2", "h1"), Attachment("T1", "E1", "h2"), Attachment("T2", "E2", "h2") };

        var result = _rule.Run(docs);

        Assert.Equal(new[] { "E1", "E2", "T1", "T2" }, Ids(result));
    }

    [Fact]
    public void EmptyInput_ReturnsEmpty()
    {
        Assert.Empty(_rule.Run(Array.Empty<Document>()));
    }
}
