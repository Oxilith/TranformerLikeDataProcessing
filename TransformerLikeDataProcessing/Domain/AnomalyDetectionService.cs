namespace TransformerLikeDataProcessing.Domain;

public class AnomalyDetectionService(double zThreshold) : IAnomalyDetectionService
{
    public double ZThreshold { get; } = zThreshold;

    public bool IsAnomalous(Employee employee, EmbeddingVector groupAverage, EmbeddingVector groupStdDev)
    {
        if (employee.AggregatedEmbedding == EmbeddingVector.Empty) return false;

        var maxZ = 0.0;
        var rowValues = employee.AggregatedEmbedding.Values;
        for (var i = 0; i < rowValues.Length; i++)
        {
            var stdDev = groupStdDev.Values[i];
            var diff = rowValues[i] - groupAverage.Values[i];
            var z = stdDev > 0 ? Math.Abs(diff) / stdDev : 0.0;

            if (z > maxZ) maxZ = z;
        }

        return maxZ > ZThreshold;
    }
}