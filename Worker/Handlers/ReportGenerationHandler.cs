using Application.Common.Abstracts;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Worker.Handlers;

public class ReportGenerationHandler : OperationHandler
{
    public ReportGenerationHandler(IErrorSimulator errorSimulator) : base(errorSimulator)
    {
        OperationType = OperationTypeEnum.ReportGeneration;
    }

    public override async Task<string> HandleAsync(IApplicationDbContext context, OperationRecord operation, CancellationToken cancellationToken)
    {
        await MakeProgress(context, operation, 5);
        await Task.Delay(TimeSpan.FromSeconds(11), cancellationToken);
        await MakeProgress(context, operation, 32);
        await Task.Delay(TimeSpan.FromSeconds(19), cancellationToken);
        await MakeProgress(context, operation, 50);
        await Task.Delay(TimeSpan.FromSeconds(9), cancellationToken);
        await MakeProgress(context, operation, 79);
        if (ErrorSimulator.ShouldSimulateError())
        {
            throw new Exception("Report generation failed due to an unexpected error.");
        }
        await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        await MakeProgress(context, operation, 100);
        return """{"report_url":"/reports/2024-Q1.pdf","pages":12}""";
    }
}