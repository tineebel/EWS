using EWS.API.Models;
using EWS.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace EWS.API.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Success<T>(T data) =>
        Ok(JsendResponse.Success(data));

    protected IActionResult Created<T>(T data, string? location = null)
    {
        var response = JsendResponse.Success(data);
        return location != null
            ? base.Created(location, response)
            : StatusCode(201, response);
    }

    protected IActionResult Fail(string message, object? data = null) =>
        BadRequest(JsendResponse.Fail(message, data));

    protected IActionResult NotFound(string message) =>
        base.NotFound(JsendResponse.Fail(message));

    protected IActionResult FromResult<T>(Result<T> result) =>
        result.IsSuccess
            ? Success(result.Value!)
            : result.ErrorCode?.EndsWith("NOT_FOUND") == true
                ? NotFound(result.Error!)
                : Fail($"[{result.ErrorCode}] {result.Error}");

    protected IActionResult FromResult(Result result) =>
        result.IsSuccess
            ? Success(new { })
            : Fail($"[{result.ErrorCode}] {result.Error}");

    protected IActionResult Paginated<T>(PaginatedList<T> list) =>
        Ok(JsendResponse.Success(new PaginatedResponse<T>
        {
            Items = list.Items,
            Page = list.Page,
            PageSize = list.PageSize,
            TotalRows = list.TotalRows,
            TotalPage = list.TotalPage
        }));
}
