using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Interfaces.Repositories;

namespace GestionProyecto.Application.UseCases.Tareas;

public class ObtenerTareasVencidasUseCase
{
    private readonly ITareaRepository _tareaRepository;

    public ObtenerTareasVencidasUseCase(
        ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    public async Task<IReadOnlyList<TareaDto>> EjecutarAsync()
    {
        var tareas = await _tareaRepository.ObtenerVencidasAsync();

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