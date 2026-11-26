using System.Text.Json;
using System.Text.Json.Serialization;
using GestionProyecto.Api.Controllers;
using GestionProyecto.Api.Middleware;
using GestionProyecto.Application.Extensions;
using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Infrastructure.Persistence;
using GestionProyecto.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace GestionProyecto.IntegrationTests;

/// <summary>
/// Levanta la API completa sobre Kestrel en un puerto efímero con EF Core InMemory para pruebas end-to-end.
/// </summary>
public sealed class ApiTestFactory : IAsyncLifetime
{
    private WebApplication? _app;

    public HttpClient Client { get; private set; } = null!;

    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task InitializeAsync()
    {
        var dbName = $"GestionProyecto_Integration_{Guid.NewGuid()}";

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.WebHost.UseUrls("http://127.0.0.1:0");

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(ProyectosController).Assembly)
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddApplication();

        builder.Services.AddDbContext<GestionProyectoDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        builder.Services.AddScoped<IProyectoRepository, ProyectoRepository>();
        builder.Services.AddScoped<ITareaRepository, TareaRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        _app = builder.Build();

        _app.UseMiddleware<ExceptionHandlingMiddleware>();
        _app.UseSwagger();
        _app.UseAuthorization();
        _app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
        _app.MapControllers();

        await _app.StartAsync();

        var baseAddress = _app.Urls.First();
        Client = new HttpClient { BaseAddress = new Uri(baseAddress) };
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();

        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
