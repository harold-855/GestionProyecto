using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Interfaces.Repositories;

namespace GestionProyecto.Application.UseCases.Tareas;

public class ObtenerTareasUseCase
{
    private readonly ITareaRepository _tareaRepository;

    public ObtenerTareasUseCase(
        ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<IReadOnlyList<TareaDto>> EjecutarAsync()
    {
        // 1. Obtener todas las tareas.
        var tareas = await _tareaRepository.ObtenerTodasAsync();

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

        // 3. Devolver la lista.
        return respuesta;
    }
}