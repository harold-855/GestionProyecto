using GestionProyecto.Domain.Entities;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Application.Interfaces.Repositories;

public interface ITareaRepository
{
    // Obtener una tarea por su identificador.
    Task<Tarea?> ObtenerPorIdAsync(Guid id);

    // Obtener todas las tareas.
    Task<IReadOnlyList<Tarea>> ObtenerTodasAsync();

    // Obtener las tareas pertenecientes a un proyecto.
    Task<IReadOnlyList<Tarea>> ObtenerPorProyectoAsync(Guid proyectoId);

    // Filtrar tareas por estado.
    Task<IReadOnlyList<Tarea>> ObtenerPorEstadoAsync(EstadoTarea estado);

    // Filtrar tareas por prioridad.
    Task<IReadOnlyList<Tarea>> ObtenerPorPrioridadAsync(PrioridadTarea prioridad);

    // Obtener tareas vencidas que no estén completadas.
    Task<IReadOnlyList<Tarea>> ObtenerVencidasAsync();

    // Registrar una tarea.
    Task AgregarAsync(Tarea tarea);

    // Actualizar una tarea.
    void Actualizar(Tarea tarea);

    // Eliminar una tarea.
    void Eliminar(Tarea tarea);
}