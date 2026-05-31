using Application.Logic.Operation.Queries.GetOperation;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationPlatform.Models.Operation.Request;

public class GetOperationRequestApiModel : IRequestApiModel<GetOperationQuery>
{
    [FromRoute]
    public Guid Id { get; set; }
    
    public GetOperationQuery Map()
    {
        return new GetOperationQuery()
        {
            Id = Id
        };
    }
}