namespace TransformerLikeDataProcessing;

using Presentation;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var csvFilePath = "data.csv";
        var embeddingFields = new List<string> { "Age", "Department", "JobTitle", "Salary" };

        var controller = new EmployeeController();
        await controller.ProcessEmployeeDataAsync(csvFilePath, embeddingFields);
    }
}