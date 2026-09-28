using HrCrm.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace HrCrm.WebApi.Controllers;

[ApiController]
public abstract class ApiBaseController : ControllerBase
{
    protected ActionResult ToErrorResponse(Result result)
    {
        var (statusCode, title) = result.ErrorType switch
        {
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource not found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Operation failed"),
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation failed"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Authentication failed"),
            _ => (StatusCodes.Status400BadRequest, "Request failed")
        };

        return Problem(statusCode: statusCode, title: title, detail: result.Error);
    }
}
