namespace Application.Interfaces;

public interface IOperationQueue
{
    Task EnqueueAsync(Guid operationId, CancellationToken ct = default);
    Task<Guid?> DequeueAsync(CancellationToken ct = default);
}