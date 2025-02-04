namespace TranformerLikeDataProcessing.Domain;

public class Employee(
    string name,
    string department,
    int age,
    decimal salary,
    string jobTitle,
    string additionalNotes,
    EmbeddingVector aggregatedEmbedding)
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; } = name;
    public string Department { get; } = department;
    public int Age { get; } = age;
    public decimal Salary { get; } = salary;
    public string JobTitle { get; } = jobTitle;
    public string AdditionalNotes { get; } = additionalNotes;

    // Per-field embeddings for diagnosing which field is anomalous.
    public Dictionary<string, EmbeddingVector> FieldEmbeddings { get; } = new();

    // Aggregated embedding for entire row.
    public EmbeddingVector AggregatedEmbedding { get; private set; } = aggregatedEmbedding;

    public void SetAggregatedEmbedding(EmbeddingVector embedding)
    {
        AggregatedEmbedding = embedding;
    }
}