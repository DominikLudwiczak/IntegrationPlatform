using Application.Common.Abstracts;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Worker.Handlers;

public class WebhookDispatchHandler : OperationHandler
{
    public WebhookDispatchHandler(IErrorSimulator errorSimulator) : base(errorSimulator)
    {
        OperationType = OperationTypeEnum.WebhookDispatch;
    }

    public override async Task<string> HandleAsync(IApplicationDbContext context, OperationRecord operation, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        if (ErrorSimulator.ShouldSimulateError())
        {
            throw new Exception("Webhook dispatch failed due to unknown error.");
        }
        await MakeProgress(context, operation, 100);
        return """{"status":200,"delivered":true}""";
    }
}