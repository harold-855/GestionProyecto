using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.UseCases.Tareas;

public class FiltrarTareasPorPrioridadUseCase
{
    private readonly ITareaRepository _tareaRepository;

    public FiltrarTareasPorPrioridadUseCase(
        ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<IReadOnlyList<TareaDto>> EjecutarAsync(
        PrioridadTarea prioridad)
    {
        // 1. Consultar las tareas según su prioridad.
        var tareas = await _tareaRepository.ObtenerPorPrioridadAsync(prioridad);

        // 2. Convertir las entidades en DTOs.
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

        // 3. Devolver los resultados.
        return respuesta;
    }
}