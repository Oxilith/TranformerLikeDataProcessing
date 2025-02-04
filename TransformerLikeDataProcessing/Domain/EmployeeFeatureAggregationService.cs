namespace TransformerLikeDataProcessing.Domain;

public class EmployeeFeatureAggregationService(IEmbeddingService embeddingService)
{
    // Compute and store embeddings for each requested field, then sum them for an overall row-level embedding.
    public void AggregateEmployeeFeatures(Employee employee, IEnumerable<string> embeddingFields)
    {
        var aggregatedValues = new List<double>();

        foreach (var field in embeddingFields)
        {
            var embedding = field switch
            {
                "Age" => new EmbeddingVector([Normalizer.Normalize(employee.Age)]),
                "Salary" => new EmbeddingVector([Normalizer.Normalize((double)employee.Salary)]),
                "Department" => embeddingService.ComputeFieldEmbedding(employee.Department),
                "JobTitle" => embeddingService.ComputeFieldEmbedding(employee.JobTitle),
                _ => null
            };

            if (embedding == null) continue;

            // Store per-field embedding for later anomaly explanation.
            employee.FieldEmbeddings[field] = embedding;

            // Either initialize or increment the aggregated vector.
            if (aggregatedValues.Count == 0)
                aggregatedValues = embedding.Values.ToList();
            else
                for (var i = 0; i < aggregatedValues.Count; i++)
                    aggregatedValues[i] += embedding.Values[i];
        }

        employee.SetAggregatedEmbedding(new EmbeddingVector(aggregatedValues.ToArray()));
    }
}