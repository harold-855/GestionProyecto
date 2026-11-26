using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Domain.Entities;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.Mappings;

internal static class MappingExtensions
{
    public static ProyectoDto ToDto(this Proyecto proyecto)
    {
        return new ProyectoDto
        {
            Id = proyecto.Id,
            Nombre = proyecto.Nombre,
            Descripcion = proyecto.Descripcion,
            FechaInicio = proyecto.FechaInicio,
            FechaFinalizacion = proyecto.FechaFinalizacion,
            PorcentajeAvance = proyecto.CalcularPorcentajeAvance(),
            TotalTareas = proyecto.Tareas.Count,
            TareasCompletadas = proyecto.Tareas.Count(
                t => t.Estado == EstadoTarea.Completada)
        };
    }

    public static AvanceProyectoDto ToAvanceDto(this Proyecto proyecto)
    {
        return new AvanceProyectoDto
        {
            ProyectoId = proyecto.Id,
            Nombre = proyecto.Nombre,
            TotalTareas = proyecto.Tareas.Count,
            TareasCompletadas = proyecto.Tareas.Count(
                t => t.Estado == EstadoTarea.Completada),
            PorcentajeAvance = proyecto.CalcularPorcentajeAvance()
        };
    }

    public static TareaDto ToDto(this Tarea tarea)
    {
        return new TareaDto
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
    }
}
