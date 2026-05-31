using Consumer.Common.Interfaces;
using Consumer.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Consumer.Controllers;

[ApiController]
[Route("consumer")]
public class ConsumerController : ControllerBase
{
    private readonly ISyncOperationService SyncOperationService;
    private readonly IOperationService OperationService;
    
    public ConsumerController(ISyncOperationService syncOperationService, IOperationService operationService)
    {
        SyncOperationService = syncOperationService;
        OperationService = operationService;
    }
    
    [HttpPost]
    [Route("sync")]
    public async Task<IActionResult> AddSyncOperation([FromBody] AddSyncOperationRequest request)
    {
        var result = await SyncOperationService.AddSyncOperation(request);
        return Ok(result);
    }
    
    [HttpPost]
    [Route("async")]
    public async Task<IActionResult> AddAsyncOperation([FromBody] AddAsyncOperationRequest request)
    {
        var result = await OperationService.AddOperation(request);
        return Accepted(result);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetOperations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var operations = await OperationService.GetOperations(pageNumber, pageSize);
        return Ok(operations);
    }
}