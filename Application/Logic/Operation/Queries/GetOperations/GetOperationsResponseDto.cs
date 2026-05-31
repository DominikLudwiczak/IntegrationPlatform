using Domain.Enums;

namespace Application.Logic.Operation.Queries.GetOperations;

public class GetOperationsResponseDto
{
    public Guid Id { get; set; }
    public OperationTypeEnum Type { get; set; }
    public OperationStatusEnum Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? ProcessingTimeMs { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public bool IsTimedOut { get; set; }
}