using System.Collections.Concurrent;
using System.Text;
using TranformerLikeDataProcessing.Domain;

namespace TranformerLikeDataProcessing.Application;

public class EmployeeProcessingService(
    EmployeeFeatureAggregationService aggregationService,
    IAnomalyDetectionService anomalyDetectionService)
{
    public async Task ProcessEmployeesAsync(ProcessEmployeeDataCommand command)
    {
        // 1. Compute embeddings for each employee in parallel.
        var embedTasks = command.Employees.Select(emp =>
            Task.Run(() => aggregationService.AggregateEmployeeFeatures(emp, command.EmbeddingFields))
        );
        await Task.WhenAll(embedTasks);

        // 2. Group employees (Department -> JobTitle).
        var groups = command.Employees
            .GroupBy(e => e.Department)
            .OrderBy(g => g.Key)
            .Select(deptGroup => new
            {
                Department = deptGroup.Key,
                JobGroups = deptGroup.GroupBy(e => e.JobTitle).OrderBy(j => j.Key)
            })
            .ToList();

        var departmentLogs = new ConcurrentDictionary<string, string>();

        // 3. Process each department in parallel.
        var deptTasks = groups.Select(dept => Task.Run(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Department: {dept.Department}");

            foreach (var jobGroup in dept.JobGroups)
            {
                sb.AppendLine($"  Job Title: {jobGroup.Key}");

                var groupList = jobGroup.ToList();
                var vectorLength = groupList.First().AggregatedEmbedding.Values.Length;

                // Compute group average embedding.
                var groupAggregate = new double[vectorLength];
                foreach (var emp in groupList)
                    for (var i = 0; i < vectorLength; i++)
                        groupAggregate[i] += emp.AggregatedEmbedding.Values[i];
                for (var i = 0; i < vectorLength; i++)
                    groupAggregate[i] /= groupList.Count;
                var groupAverage = new EmbeddingVector(groupAggregate);

                // Compute group std. dev. embedding.
                var sumSquares = new double[vectorLength];
                foreach (var emp in groupList)
                    for (var i = 0; i < vectorLength; i++)
                    {
                        var diff = emp.AggregatedEmbedding.Values[i] - groupAverage.Values[i];
                        sumSquares[i] += diff * diff;
                    }

                var groupStdDevArr = new double[vectorLength];
                for (var i = 0; i < vectorLength; i++)
                    groupStdDevArr[i] = Math.Sqrt(sumSquares[i] / groupList.Count);
                var groupStdDev = new EmbeddingVector(groupStdDevArr);

                // Check each employee for anomalies.
                foreach (var emp in groupList)
                    if (anomalyDetectionService.IsAnomalous(emp, groupAverage, groupStdDev))
                    {
                        var logLines = new List<string>
                        {
                            $"    Anomaly detected for Employee {emp.Name} (ID: {emp.Id})"
                        };

                        // Pinpoint which field is the primary contributor.
                        var fieldZScores = new Dictionary<string, double>();
                        foreach (var field in command.EmbeddingFields)
                        {
                            if (!emp.FieldEmbeddings.TryGetValue(field, out var empFieldEmbedding))
                                continue;

                            // Compute group field-level average & std. dev.
                            var dim = empFieldEmbedding.Values.Length;
                            var fieldAggregate = new double[dim];
                            foreach (var e in groupList)
                                if (e.FieldEmbeddings.TryGetValue(field, out var eFieldEmbedding))
                                    for (var i = 0; i < dim; i++)
                                        fieldAggregate[i] += eFieldEmbedding.Values[i];

                            for (var i = 0; i < dim; i++)
                                fieldAggregate[i] /= groupList.Count;
                            var groupFieldAvg = new EmbeddingVector(fieldAggregate);

                            var fieldSumSquares = new double[dim];
                            foreach (var e in groupList)
                                if (e.FieldEmbeddings.TryGetValue(field, out var eFieldEmbedding))
                                    for (var i = 0; i < dim; i++)
                                    {
                                        var diff = eFieldEmbedding.Values[i] - groupFieldAvg.Values[i];
                                        fieldSumSquares[i] += diff * diff;
                                    }

                            var fieldStdDev = new double[dim];
                            for (var i = 0; i < dim; i++)
                                fieldStdDev[i] = Math.Sqrt(fieldSumSquares[i] / groupList.Count);
                            var groupFieldStdDev = new EmbeddingVector(fieldStdDev);

                            // Calculate dimension-wise z-scores for this field.
                            double maxZForField = 0;
                            for (var i = 0; i < dim; i++)
                            {
                                var stdDev = groupFieldStdDev.Values[i];
                                var diff = empFieldEmbedding.Values[i] - groupFieldAvg.Values[i];
                                var z = stdDev > 0 ? Math.Abs(diff) / stdDev : 0;
                                if (z > maxZForField)
                                    maxZForField = z;
                            }

                            fieldZScores[field] = maxZForField;
                        }

                        if (fieldZScores.Any())
                        {
                            // The field with the highest z-score is the primary anomaly driver.
                            var maxAnomalyField = fieldZScores.OrderByDescending(kv => kv.Value).First().Key;
                            var maxAnomalyZ = fieldZScores[maxAnomalyField];
                            logLines.Add(
                                $"      Field '{maxAnomalyField}' is the primary contributor with max z-score: {maxAnomalyZ:F3}");

                            // Optionally log more detail about that field’s dimension.
                            if (emp.FieldEmbeddings.TryGetValue(maxAnomalyField, out var anomalyEmbedding))
                            {
                                var dim = anomalyEmbedding.Values.Length;
                                var fieldAggregate = new double[dim];
                                foreach (var e in groupList)
                                    if (e.FieldEmbeddings.TryGetValue(maxAnomalyField, out var eFieldEmbedding))
                                        for (var i = 0; i < dim; i++)
                                            fieldAggregate[i] += eFieldEmbedding.Values[i];

                                for (var i = 0; i < dim; i++)
                                    fieldAggregate[i] /= groupList.Count;
                                var groupColAvg = new EmbeddingVector(fieldAggregate);

                                var fieldSumSquares = new double[dim];
                                foreach (var e in groupList)
                                    if (e.FieldEmbeddings.TryGetValue(maxAnomalyField, out var eFieldEmbedding))
                                        for (var i = 0; i < dim; i++)
                                        {
                                            var diff = eFieldEmbedding.Values[i] - groupColAvg.Values[i];
                                            fieldSumSquares[i] += diff * diff;
                                        }

                                var fieldStdDev = new double[dim];
                                for (var i = 0; i < dim; i++)
                                    fieldStdDev[i] = Math.Sqrt(fieldSumSquares[i] / groupList.Count);
                                var groupColStdDev = new EmbeddingVector(fieldStdDev);

                                var zScores = new List<double>();
                                for (var i = 0; i < dim; i++)
                                {
                                    var stdDev = groupColStdDev.Values[i];
                                    var diff = anomalyEmbedding.Values[i] - groupColAvg.Values[i];
                                    var z = stdDev > 0 ? Math.Abs(diff) / stdDev : 0;
                                    zScores.Add(z);
                                }

                                var maxZ = zScores.Max();

                                logLines.Add($"      Detailed reasoning for anomaly in field '{maxAnomalyField}':");
                                logLines.Add(
                                    $"        Row value:              [{string.Join(", ", anomalyEmbedding.Values.Select(v => v.ToString("F3")))}]");
                                logLines.Add(
                                    $"        Group average:          [{string.Join(", ", groupColAvg.Values.Select(v => v.ToString("F3")))}]");
                                logLines.Add(
                                    $"        Group standard dev:     [{string.Join(", ", groupColStdDev.Values.Select(v => v.ToString("F3")))}]");
                                logLines.Add(
                                    $"        Computed z-scores:      [{string.Join(", ", zScores.Select(z => z.ToString("F3")))}]");
                                if (anomalyDetectionService is AnomalyDetectionService ads)
                                    logLines.Add(
                                        $"        Maximum z-score:        {maxZ:F3} (Threshold = {ads.ZThreshold:F3})");
                                else
                                    logLines.Add($"        Maximum z-score:        {maxZ:F3}");
                            }
                        }

                        sb.AppendLine(string.Join(Environment.NewLine, logLines));
                    }
                    else
                    {
                        sb.AppendLine($"    Employee {emp.Name} is normal.");
                    }
            }

            departmentLogs[dept.Department] = sb.ToString();
        })).ToList();

        await Task.WhenAll(deptTasks);

        // 4. Output logs for each department in sorted order.
        foreach (var dept in departmentLogs.OrderBy(kv => kv.Key)) Console.WriteLine(dept.Value);
    }
}