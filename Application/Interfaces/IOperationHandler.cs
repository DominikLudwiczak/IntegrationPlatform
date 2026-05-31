using Domain;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

public interface IOperationHandler
{
    OperationTypeEnum OperationType { get; }
    Task<string> HandleAsync(OperationRecord operation, CancellationToken cancellationToken);
}