using Microsoft.AspNetCore.Mvc;

namespace GestionProyecto.Api.Controllers;

/// <summary>
/// Controlador base con respuestas estandarizadas ProblemDetails.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected NotFoundObjectResult NotFoundProblem(string detail)
    {
        return NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Recurso no encontrado",
            Detail = detail,
            Instance = HttpContext?.Request.Path
        });
    }
}
