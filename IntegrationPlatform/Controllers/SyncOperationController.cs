using IntegrationPlatform.Models.SyncOperation.Request;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IntegrationPlatform.Controllers;

[ApiController]
[Route("api/sync-operations")]
public class SyncOperationController : BaseController
{
    [HttpPost]
    [SwaggerOperation(Summary = "Add sync operation", Description = "Add new sync operation, returns sync operation Id when successfully performed sync operation. Sync operation is performed in sync way, so the response is returned when the sync operation is completed.")]
    [SwaggerResponse(200, "Successfully performed sync operation", typeof(Guid))]
    [SwaggerResponse(400, "Bad Request - The request could not be understood or was missing required parameters.", typeof(ProblemDetails))]
    [SwaggerResponse(408, "Request Timeout - The server timed out waiting for the request.", typeof(ProblemDetails))]
    [SwaggerResponse(409, "Conflict - The request could not be completed due to a conflict with the current state of the resource.", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Internal Server Error - An error occurred on the server.", typeof(ProblemDetails))]
    public async Task<IActionResult> AddSyncOperation(AddSyncOperationRequestApiModel model)
    {
        var command = model.Map();
        var result = await Mediator.Send(command);
        return result.Succeeded ? Ok(result.Data) : HandleError(result);
    }
}