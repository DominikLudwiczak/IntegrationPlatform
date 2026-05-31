using System.Net;
using Application.Common;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationPlatform.Controllers;

public abstract class BaseController : ControllerBase
{
    private IMediator _mediator = null!;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
    
    protected IActionResult HandleError(IErrorResponseDto errorResponse)
    {
        var statusCode = ApplicationConstants.ErrorMapping[errorResponse.Code].StatusCode;

        return statusCode switch
        {
            HttpStatusCode.NotFound => CreateProblemDetails(statusCode, errorResponse),
            HttpStatusCode.BadRequest => CreateProblemDetails(statusCode, errorResponse),
            HttpStatusCode.Unauthorized => CreateProblemDetails(statusCode, errorResponse),
            HttpStatusCode.Forbidden => CreateProblemDetails(statusCode, errorResponse),
            HttpStatusCode.InternalServerError => CreateProblemDetails(statusCode, errorResponse),
            HttpStatusCode.Conflict => CreateProblemDetails(statusCode, errorResponse),
            _ => CreateProblemDetails(HttpStatusCode.Conflict, errorResponse),
        };
    }
    
    private static IActionResult CreateProblemDetails(HttpStatusCode statusCode, IErrorResponseDto errorResponse, string? type = null)
    {
        var problemDetails = new ProblemDetails
        {
            Detail = errorResponse.Message,
            Status = (int)statusCode,
            Title = ApplicationConstants.ErrorMapping[errorResponse.Code].Message,
            Type = type ?? ApplicationConstants.HttpStatusCodeInformationMapping[statusCode]
        };

        problemDetails.Extensions.Add("internalCode", errorResponse.Code);

        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };
    }
}