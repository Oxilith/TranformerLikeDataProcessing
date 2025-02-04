namespace TranformerLikeDataProcessing.Domain;

public class EmbeddingVector(double[] values)
{
    public double[] Values { get; } = values;
    public static EmbeddingVector Empty => new([]);

    public double DistanceTo(EmbeddingVector other)
    {
        if (Values.Length != other.Values.Length)
            throw new ArgumentException("Vector lengths must match.");

        var sum = Values.Select((t, i) => t - other.Values[i]).Sum(diff => diff * diff);

        return Math.Sqrt(sum);
    }
}