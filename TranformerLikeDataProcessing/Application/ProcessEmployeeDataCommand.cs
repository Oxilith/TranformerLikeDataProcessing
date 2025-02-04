namespace TranformerLikeDataProcessing.Application;

using Domain;

public class ProcessEmployeeDataCommand(List<Employee> employees, List<string> embeddingFields)
{
    public List<Employee> Employees { get; } = employees;
    public List<string> EmbeddingFields { get; } = embeddingFields;
}