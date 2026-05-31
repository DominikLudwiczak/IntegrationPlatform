using Application.Logic.Operation.Queries.GetOperations;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationPlatform.Models.Operation.Request;

public class GetOperationsRequestApiModel : IRequestApiModel<GetOperationsQuery>
{
    [FromQuery]
    public int? PageNumber { get; set; }
    [FromQuery]
    public int? PageSize { get; set; }
    
    public GetOperationsQuery Map()
    {
        return new GetOperationsQuery()
        {
            PageNumber = PageNumber,
            PageSize = PageSize,
        };
    }
}