using GestionProyecto.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionProyecto.Infrastructure.Persistence;

public class GestionProyectoDbContext : DbContext
{
    public GestionProyectoDbContext(
        DbContextOptions<GestionProyectoDbContext> options)
        : base(options)
    {
    }

    // Representación de la tabla Proyectos.
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();

    // Representación de la tabla Tareas.
    public DbSet<Tarea> Tareas => Set<Tarea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(GestionProyectoDbContext).Assembly);
    }
}