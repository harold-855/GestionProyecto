using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Domain.Entities;
using GestionProyecto.Domain.Enums;
using GestionProyecto.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionProyecto.Infrastructure.Repositories;

public class TareaRepository : ITareaRepository
{
    private readonly GestionProyectoDbContext _context;

    public TareaRepository(GestionProyectoDbContext context)
    {
        _context = context;
    }

    // Obtener una tarea por su identificador.
    public async Task<Tarea?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Tareas
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    // Obtener todas las tareas.
    public async Task<IReadOnlyList<Tarea>> ObtenerTodasAsync()
    {
        return await _context.Tareas
            .ToListAsync();
    }

    // Obtener las tareas de un proyecto.
    public async Task<IReadOnlyList<Tarea>> ObtenerPorProyectoAsync(Guid proyectoId)
    {
        return await _context.Tareas
            .Where(t => t.ProyectoId == proyectoId)
            .ToListAsync();
    }

    // Obtener tareas por estado.
    public async Task<IReadOnlyList<Tarea>> ObtenerPorEstadoAsync(EstadoTarea estado)
    {
        return await _context.Tareas
            .Where(t => t.Estado == estado)
            .ToListAsync();
    }

    // Obtener tareas por prioridad.
    public async Task<IReadOnlyList<Tarea>> ObtenerPorPrioridadAsync(PrioridadTarea prioridad)
    {
        return await _context.Tareas
            .Where(t => t.Prioridad == prioridad)
            .ToListAsync();
    }

    // Filtrar tareas por estado y/o prioridad.
    public async Task<IReadOnlyList<Tarea>> ObtenerPorFiltroAsync(
        EstadoTarea? estado,
        PrioridadTarea? prioridad)
    {
        var query = _context.Tareas.AsQueryable();

        if (estado.HasValue)
            query = query.Where(t => t.Estado == estado.Value);

        if (prioridad.HasValue)
            query = query.Where(t => t.Prioridad == prioridad.Value);

        return await query.ToListAsync();
    }

    // Obtener tareas vencidas que no estén completadas.
    public async Task<IReadOnlyList<Tarea>> ObtenerVencidasAsync()
    {
        var ahoraUtc = DateTime.UtcNow;

        return await _context.Tareas
            .Where(t =>
                t.FechaLimite < ahoraUtc &&
                t.Estado != EstadoTarea.Completada)
            .ToListAsync();
    }

    // Agregar una tarea.
    public async Task AgregarAsync(Tarea tarea)
    {
        await _context.Tareas.AddAsync(tarea);
    }

    // Actualizar una tarea.
    public void Actualizar(Tarea tarea)
    {
        _context.Tareas.Update(tarea);
    }

    // Eliminar una tarea.
    public void Eliminar(Tarea tarea)
    {
        _context.Tareas.Remove(tarea);
    }
}