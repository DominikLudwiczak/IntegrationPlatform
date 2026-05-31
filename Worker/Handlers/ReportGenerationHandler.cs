using Application.Common.Abstracts;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Worker.Handlers;

public class ReportGenerationHandler : OperationHandler
{
    public ReportGenerationHandler()
    {
        OperationType = OperationTypeEnum.ReportGeneration;
    }

    public override async Task<string> HandleAsync(IApplicationDbContext context, OperationRecord operation, CancellationToken cancellationToken)
    {
        await MakeProgress(context, operation, 5);
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        await MakeProgress(context, operation, 32);
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        await MakeProgress(context, operation, 50);
        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        await MakeProgress(context, operation, 79);
        if (new Random().Next(0, 10) < 2)
        {
            throw new Exception("Report generation failed due to an unexpected error.");
        }
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        await MakeProgress(context, operation, 100);
        return """{"report_url":"/reports/2024-Q1.pdf","pages":12}""";
    }
}