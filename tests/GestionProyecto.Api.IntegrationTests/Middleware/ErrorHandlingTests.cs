using System.Net;
using System.Net.Http.Json;
using GestionProyecto.Api.IntegrationTests.Fixtures;
using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Api.IntegrationTests.Middleware;

public class ErrorHandlingTests
{
    [Fact]
    public async Task Validaciones_Y_Excepciones_RetornanProblemDetails()
    {
        await using var factory = new ApiTestFactory();
        await factory.InitializeAsync();
        var client = factory.Client;

        var badDatesResp = await client.PostAsJsonAsync("/api/v1/proyectos", new CrearProyectoDto
        {
            Nombre = "Fechas mal",
            Descripcion = "Desc",
            FechaInicio = DateTime.UtcNow.AddDays(5),
            FechaFinalizacion = DateTime.UtcNow
        }, ApiTestFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, badDatesResp.StatusCode);

        var badModelResp = await client.PostAsJsonAsync("/api/v1/proyectos", new CrearProyectoDto
        {
            Nombre = "",
            Descripcion = "",
            FechaInicio = DateTime.UtcNow,
            FechaFinalizacion = DateTime.UtcNow.AddDays(1)
        }, ApiTestFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, badModelResp.StatusCode);

        var missingProjectTaskResp = await client.PostAsJsonAsync("/api/v1/tareas", new CrearTareaDto
        {
            Titulo = "Huérfana",
            Descripcion = "Desc",
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(1),
            ProyectoId = Guid.NewGuid()
        }, ApiTestFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.NotFound, missingProjectTaskResp.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/v1/proyectos/{Guid.NewGuid()}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/v1/tareas/{Guid.NewGuid()}")).StatusCode);

        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/v1/tareas/estado/Invalido")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/v1/tareas/prioridad/Invalida")).StatusCode);
    }
}
