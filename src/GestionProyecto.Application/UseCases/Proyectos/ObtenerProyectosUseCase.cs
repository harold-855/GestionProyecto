using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.Interfaces.Repositories;

namespace GestionProyecto.Application.UseCases.Proyectos;

public class ObtenerProyectosUseCase
{
    private readonly IProyectoRepository _proyectoRepository;

    public ObtenerProyectosUseCase(
        IProyectoRepository proyectoRepository)
    {
        _proyectoRepository = proyectoRepository;
    }

    public async Task<IReadOnlyList<ProyectoDto>> EjecutarAsync()
    {
        // 1. Consultar todos los proyectos.
        var proyectos = await _proyectoRepository.ObtenerTodosAsync();

        // 2. Convertir las entidades en DTOs.
        var respuesta = proyectos.Select(proyecto => new ProyectoDto
        {
            Id = proyecto.Id,
            Nombre = proyecto.Nombre,
            Descripcion = proyecto.Descripcion,
            FechaInicio = proyecto.FechaInicio,
            FechaFinalizacion = proyecto.FechaFinalizacion,
            PorcentajeAvance = proyecto.CalcularPorcentajeAvance(),
            TotalTareas = proyecto.Tareas.Count,
            TareasCompletadas = proyecto.Tareas.Count(
                tarea => tarea.Estado ==
                         Domain.Enums.EstadoTarea.Completada)
        }).ToList();

        // 3. Devolver los resultados.
        return respuesta;
    }
}