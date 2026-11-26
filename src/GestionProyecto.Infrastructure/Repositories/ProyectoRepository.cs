using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Domain.Entities;
using GestionProyecto.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionProyecto.Infrastructure.Repositories;

public class ProyectoRepository : IProyectoRepository
{
    private readonly GestionProyectoDbContext _context;

    public ProyectoRepository(GestionProyectoDbContext context)
    {
        _context = context;
    }

    // Obtener un proyecto por su identificador.
    public async Task<Proyecto?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Proyectos
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Obtener un proyecto incluyendo sus tareas.
    public async Task<Proyecto?> ObtenerPorIdConTareasAsync(Guid id)
    {
        return await _context.Proyectos
            .Include(p => p.Tareas)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Obtener todos los proyectos incluyendo sus tareas.
    public async Task<IReadOnlyList<Proyecto>> ObtenerTodosAsync()
    {
        return await _context.Proyectos
            .Include(p => p.Tareas)
            .ToListAsync();
    }

    // Agregar un proyecto.
    public async Task AgregarAsync(Proyecto proyecto)
    {
        await _context.Proyectos.AddAsync(proyecto);
    }

    // Actualizar un proyecto.
    public void Actualizar(Proyecto proyecto)
    {
        _context.Proyectos.Update(proyecto);
    }

    // Eliminar un proyecto.
    public void Eliminar(Proyecto proyecto)
    {
        _context.Proyectos.Remove(proyecto);
    }
}