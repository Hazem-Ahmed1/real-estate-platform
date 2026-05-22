using Microsoft.AspNetCore.Http;

namespace APILayer.Controllers;

[ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(VaildationErrorResponse), StatusCodes.Status400BadRequest)]
[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
}
