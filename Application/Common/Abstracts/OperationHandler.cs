using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Abstracts;

public abstract class OperationHandler()
{
    public OperationTypeEnum OperationType { get; set; }

    public abstract Task<string> HandleAsync(IApplicationDbContext context, OperationRecord operation, CancellationToken cancellationToken);
    
    protected async Task MakeProgress(IApplicationDbContext context, OperationRecord operation, int progress)
    {
        operation.Progress = progress;
        context.OperationRecords.Update(operation);
        await context.SaveChangesAsync(CancellationToken.None);
    }
}