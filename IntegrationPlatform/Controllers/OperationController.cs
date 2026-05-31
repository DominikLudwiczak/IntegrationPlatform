using Application.Common.Models;
using Application.Logic.Operation.Queries.GetOperations;
using IntegrationPlatform.Models.Operation.Request;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IntegrationPlatform.Controllers;

[ApiController]
[Route("api/operations")]
public class OperationController : BaseController
{
    [HttpPost]
    [SwaggerOperation(Summary = "Add operation", Description = "Add new oparation")]
    [SwaggerResponse(202, "Successfully added new operation", typeof(Unit))]
    [SwaggerResponse(400, "Bad Request - The request could not be understood or was missing required parameters.", typeof(ProblemDetails))]
    public async Task<IActionResult> AddNote(AddOperationRequestApiModel model)
    {
        var command = model.Map();
        var result = await Mediator.Send(command);
        return result.Succeeded ? Accepted() : HandleError(result);
    }
    
    [HttpGet]
    [SwaggerOperation(Summary = "Get operations", Description = "Get list of operations")]
    [SwaggerResponse(200, "Successfully retrieved list of operations", typeof(PaginatedListDto<GetOperationsResponseDto>))]
    [SwaggerResponse(400, "Bad Request - The request could not be understood or was missing required parameters.", typeof(ProblemDetails))]
    public async Task<IActionResult> GetOperations([FromQuery] GetOperationsRequestApiModel model)
    {
        var query = model.Map();
        var result = await Mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet]
    [Route("{id}")]
    [SwaggerOperation(Summary = "Get operation", Description = "Get specific operation by Id")]
    [SwaggerResponse(200, "Successfully retrieved specific operation by Id", typeof(GetOperationsResponseDto))]
    [SwaggerResponse(400, "Bad Request - The request could not be understood or was missing required parameters.", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Not Found - The requested resource could not be found.", typeof(ProblemDetails))]
    public async Task<IActionResult> GetOperation(GetOperationRequestApiModel model)
    {
        var query = model.Map();
        var result = await Mediator.Send(query);
        return result.Succeeded ? Ok(result.Data) : HandleError(result);
    }
    
    [HttpPut]
    [Route("retry/{id}")]
    [SwaggerOperation(Summary = "Retry operation", Description = "Retry specific operation by Id")]
    [SwaggerResponse(202, "Successfully retried specific operation by Id", typeof(Unit))]
    [SwaggerResponse(400, "Bad Request - The request could not be understood or was missing required parameters.", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Not Found - The requested resource could not be found.", typeof(ProblemDetails))]
    public async Task<IActionResult> RetryOperation(RetryOperationRequestApiModel model)
    {
        var command = model.Map();
        var result = await Mediator.Send(command);
        return result.Succeeded ? Accepted() : HandleError(result);
    }
}