using System.Text.Json.Serialization;

namespace Domain.Enums;

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