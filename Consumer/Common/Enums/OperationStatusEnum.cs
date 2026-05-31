using System.Text.Json.Serialization;

namespace Consumer.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationStatusEnum
{
    Pending,
    Running,
    Completed,
    Failed,
    TimedOut,
    Cancelled
}