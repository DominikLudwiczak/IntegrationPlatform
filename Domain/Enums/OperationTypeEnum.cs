using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationTypeEnum
{
    DataSync,
    ReportGeneration,
    WebhookDispatch,
}