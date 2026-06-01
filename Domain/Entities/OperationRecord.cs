using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

public class OperationRecord
{
    private int _progress;
    private int _retryCount;
    
    public Guid Id { get; set; }
    public OperationTypeEnum Type { get; set; }
    public string? Payload { get; set; }
    public OperationStatusEnum Status { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    [Column("Progress")]
    public int Progress
    {
        get => _progress;
        set
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException(nameof(Progress), "Progress must be between 0 and 100");
            _progress = value;
        }
    }
    public long? ProcessingTimeMs { get; set; }
    [Column("RetryCount")]
    public int RetryCount
    {
        get => _retryCount;
        set
        {
            if (value > MaxRetries)
                throw new InvalidOperationException($"RetryCount ({value}) cannot exceed MaxRetries ({MaxRetries})");
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(RetryCount), "RetryCount cannot be negative");
            _retryCount = value;
        }
    }
    public int MaxRetries { get; set; }
    [DefaultValue(false)]
    public bool IsTimedOut { get; set; }
}