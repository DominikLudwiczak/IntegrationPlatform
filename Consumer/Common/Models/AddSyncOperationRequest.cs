using Consumer.Common.Enums;

namespace Consumer.Common.Models;

public class AddSyncOperationRequest
{
    public OperationTypeEnum Type { get; set; }
    public string? Payload { get; set; }
    public int TimeoutInSeconds { get; set; }
}