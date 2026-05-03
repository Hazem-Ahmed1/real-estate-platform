using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Common;
using Microsoft.AspNetCore.Http;

namespace APILayer.Factories;

public class ApiResponseFactory
{
    public static IActionResult CustumValidationErrorResponse(ActionContext context)
    {
        var errors = context.ModelState
            .Where(error => error.Value?.Errors.Any() == true)
            .Select(error => new VaildationError
            (
                error.Key,
                error.Value?.Errors.Select(e => e.ErrorMessage) ?? new List<string>()
            ));

        var response = new VaildationErrorResponse
        (
            StatusCodes.Status400BadRequest,
            "One or More validation error happened",
            errors
        );

        return new BadRequestObjectResult(response);
    }
}
