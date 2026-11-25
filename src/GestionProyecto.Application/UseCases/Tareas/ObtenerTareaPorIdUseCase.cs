using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Interfaces.Repositories;

namespace GestionProyecto.Application.UseCases.Tareas;

public class ObtenerTareaPorIdUseCase
{
    private readonly ITareaRepository _tareaRepository;

    public ObtenerTareaPorIdUseCase(
        ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<TareaDto?> EjecutarAsync(Guid id)
    {
        // 1. Buscar la tarea por su identificador.
        var tarea = await _tareaRepository.ObtenerPorIdAsync(id);

        // 2. Comprobar si existe.
        if (tarea is null)
            return null;

        // 3. Convertir la entidad en un DTO.
        var respuesta = new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Estado = tarea.Estado,
            Prioridad = tarea.Prioridad,
            FechaLimite = tarea.FechaLimite,
            ProyectoId = tarea.ProyectoId,
            EstaVencida = tarea.EstaVencida()
        };

        // 4. Devolver el resultado.
        return respuesta;
    }
}