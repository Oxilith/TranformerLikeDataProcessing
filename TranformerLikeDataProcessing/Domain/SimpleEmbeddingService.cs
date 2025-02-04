using System.Security.Cryptography;
using System.Text;

namespace TranformerLikeDataProcessing.Domain;

public class SimpleEmbeddingService : IEmbeddingService
{
    public EmbeddingVector ComputeFieldEmbedding(object? fieldValue)
    {
        var input = fieldValue?.ToString() ?? string.Empty;
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        
        // Take first 4 bytes -> 4-dim embedding
        var vector = new double[4];
        for (var i = 0; i < vector.Length; i++) vector[i] = hashBytes[i] / 255.0;
        
        return new EmbeddingVector(vector);
    }
}