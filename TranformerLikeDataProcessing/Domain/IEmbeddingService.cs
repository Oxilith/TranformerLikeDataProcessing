namespace TranformerLikeDataProcessing.Domain;

public interface IEmbeddingService
{
    // Returns an embedding (continuous vector) for a discrete or string field.
    EmbeddingVector ComputeFieldEmbedding(object fieldValue);
}