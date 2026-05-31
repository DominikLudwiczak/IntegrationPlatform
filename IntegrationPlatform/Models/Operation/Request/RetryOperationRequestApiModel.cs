using Application.Logic.Operation.Commands;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationPlatform.Models.Operation.Request;

public class RetryOperationRequestApiModel : IRequestApiModel<RetryOperationCommand>
{
    [FromRoute]
    public Guid Id { get; set; }
    
    public RetryOperationCommand Map()
    {
        return new RetryOperationCommand()
        {
            Id = Id
        };
    }
}