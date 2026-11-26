using GestionProyecto.Application.UseCases.Proyectos;
using GestionProyecto.Application.UseCases.Tareas;
using Microsoft.Extensions.DependencyInjection;

namespace GestionProyecto.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // =====================================
        // CASOS DE USO DE PROYECTOS
        // =====================================

        services.AddScoped<CrearProyectoUseCase>();
        services.AddScoped<ObtenerProyectosUseCase>();
        services.AddScoped<ObtenerProyectoPorIdUseCase>();
        services.AddScoped<ActualizarProyectoUseCase>();
        services.AddScoped<EliminarProyectoUseCase>();


        // =====================================
        // CASOS DE USO DE TAREAS
        // =====================================

        services.AddScoped<CrearTareaUseCase>();
        services.AddScoped<ObtenerTareasUseCase>();
        services.AddScoped<ObtenerTareaPorIdUseCase>();
        services.AddScoped<ActualizarTareaUseCase>();
        services.AddScoped<CambiarEstadoTareaUseCase>();
        services.AddScoped<ObtenerTareasPorProyectoUseCase>();
        services.AddScoped<FiltrarTareasPorEstadoUseCase>();
        services.AddScoped<FiltrarTareasPorPrioridadUseCase>();
        services.AddScoped<ObtenerTareasVencidasUseCase>();
        services.AddScoped<EliminarTareaUseCase>();

        return services;
    }
}