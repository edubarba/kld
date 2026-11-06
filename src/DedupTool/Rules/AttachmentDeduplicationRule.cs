namespace DedupTool.Rules;

public class AttachmentDeduplicationRule : IDeduplicationRule
{
    private const string AttachmentDocType = "attachment";

    public IReadOnlyList<Document> Run(IReadOnlyList<Document> documents)
    {
        var survivorByHash = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in documents)
        {
            if (d.DocType != AttachmentDocType) continue;

            if (!survivorByHash.TryGetValue(d.Sha256, out var current)
                || string.CompareOrdinal(d.DocId, current) < 0)
            {
                survivorByHash[d.Sha256] = d.DocId;
            }
        }

        return documents
            .Where(d => d.DocType != AttachmentDocType || survivorByHash[d.Sha256] == d.DocId)
            .ToList();
    }
}
