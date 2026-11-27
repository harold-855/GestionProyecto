using GestionProyecto.Api.Extensions;
using GestionProyecto.Api.Middleware;
using GestionProyecto.Application.Extensions;
using GestionProyecto.Infrastructure.Extensions;
using GestionProyecto.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registrar servicios de la API, rutas versionadas en minúsculas y Swagger.
builder.Services.AddApiPresentation();

// Registrar Application.
builder.Services.AddApplication();

// Registrar Infrastructure.
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Aplicar migraciones automáticamente si está habilitado (ej. despliegue en Azure).
if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<GestionProyectoDbContext>();
    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync();
    }
}

// Middleware global de manejo de excepciones.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Habilitar Swagger tanto en desarrollo como en despliegue (Azure).
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gestión de Proyectos y Tareas v1");
});

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestampUtc = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.MapControllers();

app.Run();

public partial class Program { }
