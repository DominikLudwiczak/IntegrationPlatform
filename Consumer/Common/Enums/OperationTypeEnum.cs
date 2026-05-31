using System.Text.Json.Serialization;

namespace Consumer.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationTypeEnum
{
    DataSync,
    ReportGeneration,
    WebhookDispatch,
}