using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Worker.Handlers;

public class ReportGenerationHandler : IOperationHandler
{
    public OperationTypeEnum OperationType => OperationTypeEnum.ReportGeneration;

    public async Task<string> HandleAsync(OperationRecord operation, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        return """{"report_url":"/reports/2024-Q1.pdf","pages":12}""";
    }
}