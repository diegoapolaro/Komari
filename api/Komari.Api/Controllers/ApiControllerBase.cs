using Microsoft.AspNetCore.Mvc;

namespace Komari.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
}
