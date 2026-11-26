using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.UseCases.Tareas;
using GestionProyecto.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GestionProyecto.Api.Controllers;

[Route("api/[controller]")]
public class TareasController : ApiControllerBase
{
    private readonly CrearTareaUseCase _crearTarea;
    private readonly ObtenerTareasUseCase _obtenerTareas;
    private readonly ObtenerTareaPorIdUseCase _obtenerTareaPorId;
    private readonly ActualizarTareaUseCase _actualizarTarea;
    private readonly CambiarEstadoTareaUseCase _cambiarEstadoTarea;
    private readonly FiltrarTareasUseCase _filtrarTareas;
    private readonly FiltrarTareasPorEstadoUseCase _filtrarPorEstado;
    private readonly FiltrarTareasPorPrioridadUseCase _filtrarPorPrioridad;
    private readonly ObtenerTareasVencidasUseCase _obtenerVencidas;
    private readonly EliminarTareaUseCase _eliminarTarea;

    public TareasController(
        CrearTareaUseCase crearTarea,
        ObtenerTareasUseCase obtenerTareas,
        ObtenerTareaPorIdUseCase obtenerTareaPorId,
        ActualizarTareaUseCase actualizarTarea,
        CambiarEstadoTareaUseCase cambiarEstadoTarea,
        FiltrarTareasUseCase filtrarTareas,
        FiltrarTareasPorEstadoUseCase filtrarPorEstado,
        FiltrarTareasPorPrioridadUseCase filtrarPorPrioridad,
        ObtenerTareasVencidasUseCase obtenerVencidas,
        EliminarTareaUseCase eliminarTarea)
    {
        _crearTarea = crearTarea;
        _obtenerTareas = obtenerTareas;
        _obtenerTareaPorId = obtenerTareaPorId;
        _actualizarTarea = actualizarTarea;
        _cambiarEstadoTarea = cambiarEstadoTarea;
        _filtrarTareas = filtrarTareas;
        _filtrarPorEstado = filtrarPorEstado;
        _filtrarPorPrioridad = filtrarPorPrioridad;
        _obtenerVencidas = obtenerVencidas;
        _eliminarTarea = eliminarTarea;
    }

    /// <summary>Lista todas las tareas, opcionalmente filtradas por estado y/o prioridad.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TareaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TareaDto>>> ObtenerTodas(
        [FromQuery] EstadoTarea? estado,
        [FromQuery] PrioridadTarea? prioridad)
    {
        if (estado.HasValue || prioridad.HasValue)
        {
            var filtradas = await _filtrarTareas.EjecutarAsync(estado, prioridad);
            return Ok(filtradas);
        }

        var tareas = await _obtenerTareas.EjecutarAsync();
        return Ok(tareas);
    }

    /// <summary>Obtiene una tarea por su identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaDto>> ObtenerPorId(Guid id)
    {
        var tarea = await _obtenerTareaPorId.EjecutarAsync(id);
        if (tarea is null)
            return NotFoundProblem($"No se encontró la tarea con ID '{id}'.");

        return Ok(tarea);
    }

    /// <summary>Filtra tareas por estado.</summary>
    [HttpGet("estado/{estado}")]
    [ProducesResponseType(typeof(IReadOnlyList<TareaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TareaDto>>> FiltrarPorEstado(EstadoTarea estado)
    {
        var tareas = await _filtrarPorEstado.EjecutarAsync(estado);
        return Ok(tareas);
    }

    /// <summary>Filtra tareas por prioridad.</summary>
    [HttpGet("prioridad/{prioridad}")]
    [ProducesResponseType(typeof(IReadOnlyList<TareaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TareaDto>>> FiltrarPorPrioridad(PrioridadTarea prioridad)
    {
        var tareas = await _filtrarPorPrioridad.EjecutarAsync(prioridad);
        return Ok(tareas);
    }

    /// <summary>Consulta tareas vencidas que no estén completadas.</summary>
    [HttpGet("vencidas")]
    [ProducesResponseType(typeof(IReadOnlyList<TareaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TareaDto>>> ObtenerVencidas()
    {
        var tareas = await _obtenerVencidas.EjecutarAsync();
        return Ok(tareas);
    }

    /// <summary>Crea una nueva tarea asociada a un proyecto.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TareaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaDto>> Crear(
        [FromBody] CrearTareaDto dto,
        CancellationToken cancellationToken)
    {
        var tarea = await _crearTarea.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = tarea.Id }, tarea);
    }

    /// <summary>Actualiza los datos de una tarea existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarTareaDto dto,
        CancellationToken cancellationToken)
    {
        var actualizado = await _actualizarTarea.EjecutarAsync(id, dto, cancellationToken);
        if (!actualizado)
            return NotFoundProblem($"No se encontró la tarea con ID '{id}'.");

        return NoContent();
    }

    /// <summary>Cambia el estado de una tarea.</summary>
    [HttpPatch("{id:guid}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(
        Guid id,
        [FromBody] CambiarEstadoTareaDto dto,
        CancellationToken cancellationToken)
    {
        var actualizado = await _cambiarEstadoTarea.EjecutarAsync(id, dto.Estado, cancellationToken);
        if (!actualizado)
            return NotFoundProblem($"No se encontró la tarea con ID '{id}'.");

        return NoContent();
    }

    /// <summary>Elimina una tarea existente.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var eliminado = await _eliminarTarea.EjecutarAsync(id, cancellationToken);
        if (!eliminado)
            return NotFoundProblem($"No se encontró la tarea con ID '{id}'.");

        return NoContent();
    }
}
