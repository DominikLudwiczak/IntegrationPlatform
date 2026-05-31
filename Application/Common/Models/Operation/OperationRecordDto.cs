using Domain.Enums;

namespace Application.Common.Models.Operation;

public class OperationRecordDto
{
    public Guid Id { get; set; }
    public OperationTypeEnum Type { get; set; }
    public string? Payload { get; set; }
    public OperationStatusEnum Status { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int Progress { get; set; }
    public long? ProcessingTimeMs { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public bool IsTimedOut { get; set; }
}