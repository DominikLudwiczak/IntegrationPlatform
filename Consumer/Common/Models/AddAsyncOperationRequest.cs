using Consumer.Common.Enums;

namespace Consumer.Common.Models;

public class AddAsyncOperationRequest
{
    public OperationTypeEnum Type { get; set; }
    public string? Payload { get; set; }
    public int MaxRetries { get; set; }
}