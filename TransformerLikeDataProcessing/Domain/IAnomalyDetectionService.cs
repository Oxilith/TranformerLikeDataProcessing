namespace TransformerLikeDataProcessing.Domain;

public interface IAnomalyDetectionService
{
    bool IsAnomalous(Employee employee, EmbeddingVector groupAverage, EmbeddingVector groupStdDev);
}