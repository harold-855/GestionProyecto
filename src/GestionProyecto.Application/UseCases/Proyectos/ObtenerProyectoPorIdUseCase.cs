using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.UseCases.Proyectos;

public class ObtenerProyectoPorIdUseCase
{
    private readonly IProyectoRepository _proyectoRepository;

    public ObtenerProyectoPorIdUseCase(
        IProyectoRepository proyectoRepository)
    {
        _proyectoRepository = proyectoRepository;
    }

    public async Task<ProyectoDto?> EjecutarAsync(Guid id)
    {
        // 1. Buscar el proyecto incluyendo sus tareas.
        var proyecto = await _proyectoRepository
            .ObtenerPorIdConTareasAsync(id);

        // 2. Comprobar si existe.
        if (proyecto is null)
            return null;

        // 3. Convertir la entidad en un DTO.
        var respuesta = new ProyectoDto
        {
            Id = proyecto.Id,
            Nombre = proyecto.Nombre,
            Descripcion = proyecto.Descripcion,
            FechaInicio = proyecto.FechaInicio,
            FechaFinalizacion = proyecto.FechaFinalizacion,
            PorcentajeAvance = proyecto.CalcularPorcentajeAvance(),
            TotalTareas = proyecto.Tareas.Count,
            TareasCompletadas = proyecto.Tareas.Count(
                tarea => tarea.Estado == EstadoTarea.Completada)
        };

        // 4. Devolver el resultado.
        return respuesta;
    }
}