using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Exceptions;
using GestionProyecto.Application.Interfaces.Repositories;

namespace GestionProyecto.Application.UseCases.Tareas;

public class ObtenerTareasPorProyectoUseCase
{
    private readonly ITareaRepository _tareaRepository;
    private readonly IProyectoRepository _proyectoRepository;

    public ObtenerTareasPorProyectoUseCase(
        ITareaRepository tareaRepository,
        IProyectoRepository proyectoRepository)
    {
        _tareaRepository = tareaRepository;
        _proyectoRepository = proyectoRepository;
    }

    public async Task<IReadOnlyList<TareaDto>> EjecutarAsync(Guid proyectoId)
    {
        var proyecto = await _proyectoRepository.ObtenerPorIdAsync(proyectoId);

        if (proyecto is null)
            throw new NotFoundException(
                "No se encontró el proyecto indicado.");

        var tareas = await _tareaRepository.ObtenerPorProyectoAsync(proyectoId);

        var respuesta = tareas.Select(tarea => new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Estado = tarea.Estado,
            Prioridad = tarea.Prioridad,
            FechaLimite = tarea.FechaLimite,
            ProyectoId = tarea.ProyectoId,
            EstaVencida = tarea.EstaVencida()
        }).ToList();

        return respuesta;
    }
}