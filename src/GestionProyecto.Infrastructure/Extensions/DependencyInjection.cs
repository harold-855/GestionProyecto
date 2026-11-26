using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Infrastructure.Persistence;
using GestionProyecto.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionProyecto.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Obtener la cadena de conexión.
        var connectionString = configuration
                                   .GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "No se encontró la cadena de conexión DefaultConnection.");

        // Registrar el contexto de Entity Framework.
        services.AddDbContext<GestionProyectoDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Registrar los repositorios.
        services.AddScoped<IProyectoRepository, ProyectoRepository>();
        services.AddScoped<ITareaRepository, TareaRepository>();

        // Registrar UnitOfWork.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}