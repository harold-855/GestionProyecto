using System.Text.Json.Serialization;
using GestionProyecto.Api.Controllers;
using GestionProyecto.Api.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GestionProyecto.Api.Extensions;

/// <summary>
/// Extensiones de registro de servicios HTTP, ruteo en minúsculas y Swagger.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiPresentation(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
        });

        services.AddControllers(options =>
            {
                options.Conventions.Add(
                    new RouteTokenTransformerConvention(new LowercaseParameterTransformer()));
            })
            .AddApplicationPart(typeof(ProyectosController).Assembly)
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
