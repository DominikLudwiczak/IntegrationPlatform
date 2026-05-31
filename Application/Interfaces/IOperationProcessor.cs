namespace Application.Interfaces;

public interface IOperationProcessor
{ 
    Task<int> ProcessAsync(Guid operationId, CancellationToken cancellationToken);
}