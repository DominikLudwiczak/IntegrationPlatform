using Application.Logic.Operation.Commands;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationPlatform.Models.Operation.Request;

public class AddOperationRequestApiModel : IRequestApiModel<AddOperationCommand>
{
    [FromBody]
    public AddOperationRequestBody Body { get; set; }
    
    public AddOperationCommand Map()
    {
        return new AddOperationCommand()
        {
            Type = Body.Type,
            Payload = Body.Payload,
            MaxRetries = Body.MaxRetries        
        };
    }
}

public class AddOperationRequestBody
{
    public OperationTypeEnum Type { get; set; }
    public string? Payload { get; set; }
    public int MaxRetries { get; set; }
}
