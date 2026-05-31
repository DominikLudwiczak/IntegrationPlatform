using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Worker.Handlers;

public class DataSyncHandler : IOperationHandler
{
    public OperationTypeEnum OperationType => OperationTypeEnum.DataSync;

    public async Task<string> HandleAsync(OperationRecord operation, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        return """{"synced":142,"skipped":3,"duration_ms":2000}""";
    }
}