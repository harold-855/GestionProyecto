using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.UseCases.Proyectos;
using GestionProyecto.Application.UseCases.Tareas;
using Microsoft.AspNetCore.Mvc;

namespace GestionProyecto.Api.Controllers;

public class ProyectosController : ApiControllerBase
{
    private readonly CrearProyectoUseCase _crearProyecto;
    private readonly ObtenerProyectosUseCase _obtenerProyectos;
    private readonly ObtenerProyectoPorIdUseCase _obtenerProyectoPorId;
    private readonly ObtenerAvanceProyectoUseCase _obtenerAvanceProyecto;
    private readonly ActualizarProyectoUseCase _actualizarProyecto;
    private readonly EliminarProyectoUseCase _eliminarProyecto;
    private readonly ObtenerTareasPorProyectoUseCase _obtenerTareasPorProyecto;

    public ProyectosController(
        CrearProyectoUseCase crearProyecto,
        ObtenerProyectosUseCase obtenerProyectos,
        ObtenerProyectoPorIdUseCase obtenerProyectoPorId,
        ObtenerAvanceProyectoUseCase obtenerAvanceProyecto,
        ActualizarProyectoUseCase actualizarProyecto,
        EliminarProyectoUseCase eliminarProyecto,
        ObtenerTareasPorProyectoUseCase obtenerTareasPorProyecto)
    {
        _crearProyecto = crearProyecto;
        _obtenerProyectos = obtenerProyectos;
        _obtenerProyectoPorId = obtenerProyectoPorId;
        _obtenerAvanceProyecto = obtenerAvanceProyecto;
        _actualizarProyecto = actualizarProyecto;
        _eliminarProyecto = eliminarProyecto;
        _obtenerTareasPorProyecto = obtenerTareasPorProyecto;
    }

    /// <summary>Obtiene todos los proyectos con su porcentaje de avance.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProyectoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProyectoDto>>> ObtenerTodos()
    {
        var proyectos = await _obtenerProyectos.EjecutarAsync();
        return Ok(proyectos);
    }

    /// <summary>Obtiene un proyecto por su identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProyectoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProyectoDto>> ObtenerPorId(Guid id)
    {
        var proyecto = await _obtenerProyectoPorId.EjecutarAsync(id);
        if (proyecto is null)
            return NotFoundProblem($"No se encontró el proyecto con ID '{id}'.");

        return Ok(proyecto);
    }

    /// <summary>Obtiene el porcentaje de avance y resumen de tareas de un proyecto.</summary>
    [HttpGet("{id:guid}/avance")]
    [ProducesResponseType(typeof(AvanceProyectoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvanceProyectoDto>> ObtenerAvance(Guid id)
    {
        var avance = await _obtenerAvanceProyecto.EjecutarAsync(id);
        if (avance is null)
            return NotFoundProblem($"No se encontró el proyecto con ID '{id}'.");

        return Ok(avance);
    }

    /// <summary>Consulta todas las tareas asociadas a un proyecto.</summary>
    [HttpGet("{id:guid}/tareas")]
    [ProducesResponseType(typeof(IReadOnlyList<TareaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TareaDto>>> ObtenerTareas(Guid id)
    {
        var tareas = await _obtenerTareasPorProyecto.EjecutarAsync(id);
        return Ok(tareas);
    }

    /// <summary>Crea un nuevo proyecto.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProyectoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProyectoDto>> Crear(
        [FromBody] CrearProyectoDto dto,
        CancellationToken cancellationToken)
    {
        var proyecto = await _crearProyecto.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = proyecto.Id }, proyecto);
    }

    /// <summary>Actualiza un proyecto existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarProyectoDto dto,
        CancellationToken cancellationToken)
    {
        var actualizado = await _actualizarProyecto.EjecutarAsync(id, dto, cancellationToken);
        if (!actualizado)
            return NotFoundProblem($"No se encontró el proyecto con ID '{id}'.");

        return NoContent();
    }

    /// <summary>Elimina un proyecto si no tiene tareas pendientes.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var eliminado = await _eliminarProyecto.EjecutarAsync(id, cancellationToken);
        if (!eliminado)
            return NotFoundProblem($"No se encontró el proyecto con ID '{id}'.");

        return NoContent();
    }
}
