using GestionProyecto.Application.Interfaces;
using GestionProyecto.Infrastructure.Persistence;

namespace GestionProyecto.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GestionProyectoDbContext _context;

    public UnitOfWork(GestionProyectoDbContext context)
    {
        _context = context;
    }

    public async Task<int> GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}