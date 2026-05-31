using Application.Logic.SyncOperation.Commands;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationPlatform.Models.SyncOperation.Request;

public class AddSyncOperationRequestApiModel : IRequestApiModel<AddSyncOperationCommand>
{
    [FromBody]
    public AddSyncOperationRequestBody Body { get; set; }
    
    public AddSyncOperationCommand Map()
    {
        return new AddSyncOperationCommand()
        {
            Type = Body.Type,
            Payload = Body.Payload,
            TimeoutInSeconds = Body.TimeoutInSeconds
        };
    }
}

public class AddSyncOperationRequestBody
{
    public OperationTypeEnum Type { get; set; }
    public string? Payload { get; set; }
    public int TimeoutInSeconds { get; set; }
}
