using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Worker.Handlers;

public class WebhookDispatchHandler : IOperationHandler
{
    public OperationTypeEnum OperationType => OperationTypeEnum.WebhookDispatch;

    public async Task<string> HandleAsync(OperationRecord operation, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        return """{"status":200,"delivered":true}""";
    }
}