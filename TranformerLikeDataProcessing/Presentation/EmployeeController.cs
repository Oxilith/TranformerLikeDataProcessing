namespace TranformerLikeDataProcessing.Presentation;

using Application;
using Domain;
using Infrastructure;

public class EmployeeController
{
    private const double ZThreshold = 2.0;
    private readonly EmployeeProcessingService _processingService;

    public EmployeeController()
    {
        // Create the embedding + anomaly detection pipeline.
        IEmbeddingService embeddingService = new SimpleEmbeddingService();
        var aggregationService = new EmployeeFeatureAggregationService(embeddingService);
        IAnomalyDetectionService anomalyDetectionService = new AnomalyDetectionService(ZThreshold);
        
        _processingService = new EmployeeProcessingService(aggregationService, anomalyDetectionService);
    }

    public async Task ProcessEmployeeDataAsync(string csvFilePath, List<string> embeddingFields)
    {
        // 1. Load employees from CSV asynchronously.
        var employees = await CsvEmployeeLoader.LoadEmployeesFromCsvAsync(csvFilePath);

        // 2. Create a command object 
        var command = new ProcessEmployeeDataCommand(employees, embeddingFields);

        // 3. Execute the main processing.
        await _processingService.ProcessEmployeesAsync(command);
    }
}