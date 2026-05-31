using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace OperationProcessor;

public class OperationProcessorService : IOperationProcessor
{
    private readonly IApplicationDbContext Context;
    private readonly IEnumerable<IOperationHandler> Handlers;
    
    public OperationProcessorService(IApplicationDbContext context, IEnumerable<IOperationHandler> handlers)
    {
        Context = context;
        Handlers = handlers;
    }
    
    public async Task<int> ProcessAsync(Guid operationId, CancellationToken cancellationToken)
    {
        var operation = await Context.OperationRecords.SingleOrDefaultAsync(x => x.Id == operationId, CancellationToken.None);
        
        if (operation == null || operation.Status == OperationStatusEnum.Cancelled)
        {
            return -3;
        }

        var handler = Handlers.FirstOrDefault(h => h.OperationType == operation.Type)
                      ?? throw new InvalidOperationException($"No handler registered for type {operation.Type}.");
        
        await StartOperation(operation);

        try
        {
            var result = await handler.HandleAsync(operation, cancellationToken);
            await CompleteOperation(operation, result);
            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (operation.Status != OperationStatusEnum.Cancelled)
            {
                await TimeoutOperation(operation);
                return -5;
            }
        }
        catch (Exception ex)
        {
            await FailOperation(operation, ex.Message);
            return -4;
        }

        return -2;
    }
    
    private async Task StartOperation(OperationRecord operation)
    {
        if (operation.Status != OperationStatusEnum.Pending)
        {
            throw new InvalidOperationException($"Cannot start operation in status {operation.Status}.");
        }
        operation.Status = OperationStatusEnum.Running;
        operation.StartedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync(CancellationToken.None);
    }
    
    private async Task CompleteOperation(OperationRecord operation, string result)
    {
        operation.Status = OperationStatusEnum.Completed;
        operation.Result = result;
        operation.CompletedAt = DateTime.UtcNow;
        operation.ProcessingTimeMs = operation.StartedAt.HasValue ? (long)(operation.CompletedAt - operation.StartedAt)?.TotalMilliseconds! : null;
        await Context.SaveChangesAsync(CancellationToken.None);
    }

    private async Task FailOperation(OperationRecord operation, string errorMessage)
    {
        operation.Status = OperationStatusEnum.Failed;
        operation.ErrorMessage = errorMessage;
        operation.CompletedAt = DateTime.UtcNow;
        operation.ProcessingTimeMs = operation.StartedAt.HasValue ? (long)(operation.CompletedAt - operation.StartedAt)?.TotalMilliseconds! : null;
        await Context.SaveChangesAsync(CancellationToken.None);
    }

    private async Task TimeoutOperation(OperationRecord operation)
    {
        operation.Status = OperationStatusEnum.TimedOut;
        operation.IsTimedOut = true;
        operation.ErrorMessage = "Operation timed out.";
        operation.CompletedAt = DateTime.UtcNow;
        operation.ProcessingTimeMs = operation.StartedAt.HasValue ? (long)(operation.CompletedAt - operation.StartedAt)?.TotalMilliseconds! : null;
        await Context.SaveChangesAsync(CancellationToken.None);
    }
}