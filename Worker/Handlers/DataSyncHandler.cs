using Application.Common.Abstracts;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Worker.Handlers;

public class DataSyncHandler : OperationHandler
{
    public DataSyncHandler(IErrorSimulator errorSimulator) : base(errorSimulator)
    {
        OperationType = OperationTypeEnum.DataSync;
    }

    public override async Task<string> HandleAsync(IApplicationDbContext context, OperationRecord operation, CancellationToken cancellationToken)
    {
        await MakeProgress(context, operation, 10);
        await Task.Delay(TimeSpan.FromSeconds(19), cancellationToken);
        if (ErrorSimulator.ShouldSimulateError())
        {
            throw new Exception("Data sync failed due to a network error.");
        }
        await MakeProgress(context, operation, 50);
        await Task.Delay(TimeSpan.FromSeconds(21), cancellationToken);
        await MakeProgress(context, operation, 75);
        await Task.Delay(TimeSpan.FromSeconds(17), cancellationToken);
        await MakeProgress(context, operation, 100);
        return """{"synced":142,"skipped":3,"duration_ms":2000}""";
    }
}