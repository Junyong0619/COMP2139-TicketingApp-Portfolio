using System.Diagnostics;
using GBC_Ticketing.Models;
using Microsoft.AspNetCore.Mvc;

namespace GBC_Ticketing.Controllers;

public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("Error/{statusCode:int}")]
    public IActionResult StatusCodeHandler(int statusCode)
    {
        return BuildErrorView(statusCode);
    }

    [Route("Error/500")]
    public IActionResult ServerError()
    {
        return BuildErrorView(500);
    }

    private IActionResult BuildErrorView(int statusCode)
    {
        Response.StatusCode = statusCode;
        var viewModel = new ErrorViewModel
        {
            StatusCode = statusCode,
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        var viewName = statusCode switch
        {
            404 => "404",
            _ => "500"
        };

        _logger.LogWarning("Serving error page {StatusCode} for request {RequestId}", statusCode, viewModel.RequestId);
        return View(viewName, viewModel);
    }
}
