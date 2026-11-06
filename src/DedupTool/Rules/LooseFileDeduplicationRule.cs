namespace DedupTool.Rules;

public class LooseFileDeduplicationRule : IDeduplicationRule
{
    private const string LooseDocType = "loose";

    public IReadOnlyList<Document> Run(IReadOnlyList<Document> documents)
    {
        var survivorByHash = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in documents)
        {
            if (d.DocType != LooseDocType) continue;

            if (!survivorByHash.TryGetValue(d.Sha256, out var current)
                || string.CompareOrdinal(d.DocId, current) < 0)
            {
                survivorByHash[d.Sha256] = d.DocId;
            }
        }

        return documents
            .Where(d => d.DocType != LooseDocType || survivorByHash[d.Sha256] == d.DocId)
            .ToList();
    }
}
