using System.Globalization;

namespace TranformerLikeDataProcessing.Infrastructure;

using System.Collections.Concurrent;
using CsvHelper;
using CsvHelper.Configuration;
using Domain;

public static class CsvEmployeeLoader
{
    public static async Task<List<Employee>> LoadEmployeesFromCsvAsync(string filePath)
    {
        var employees = new ConcurrentBag<Employee>();

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            DetectDelimiter = true
        });

        // Read CSV in streaming fashion.
        var records = csv.GetRecordsAsync<EmployeeRecord>();
        await foreach (var record in records)
        {
            var employee = new Employee(
                record.Name,
                record.Department,
                record.Age,
                record.Salary,
                record.JobTitle,
                record.AdditionalNotes,
                EmbeddingVector.Empty);
            employees.Add(employee);
        }

        return employees.ToList();
    }
}
