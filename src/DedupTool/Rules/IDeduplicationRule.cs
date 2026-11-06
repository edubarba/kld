namespace DedupTool.Rules;

public interface IDeduplicationRule
{
    IReadOnlyList<Document> Run(IReadOnlyList<Document> documents);
}
