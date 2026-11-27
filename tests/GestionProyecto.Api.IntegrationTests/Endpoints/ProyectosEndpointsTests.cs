using System.Net;
using System.Net.Http.Json;
using GestionProyecto.Api.IntegrationTests.Fixtures;
using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GestionProyecto.Api.IntegrationTests.Endpoints;

public class ProyectosEndpointsTests
{
    [Fact]
    public async Task Proyectos_Crud_Y_PorcentajeDeAvance_FuncionanEndToEnd()
    {
        await using var factory = new ApiTestFactory();
        await factory.InitializeAsync();
        var client = factory.Client;

        var crearDto = new CrearProyectoDto
        {
            Nombre = "Migración Cloud",
            Descripcion = "Despliegue en Azure",
            FechaInicio = DateTime.UtcNow,
            FechaFinalizacion = DateTime.UtcNow.AddDays(30)
        };

        var createResponse = await client.PostAsJsonAsync("/api/v1/proyectos", crearDto, ApiTestFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var proyecto = await createResponse.Content.ReadFromJsonAsync<ProyectoDto>(ApiTestFactory.JsonOptions);
        Assert.NotNull(proyecto);
        Assert.Equal(0m, proyecto.PorcentajeAvance);
        Assert.Equal($"/api/v1/proyectos/{proyecto.Id}", createResponse.Headers.Location?.AbsolutePath);

        var t1Resp = await client.PostAsJsonAsync("/api/v1/tareas", new CrearTareaDto
        {
            Titulo = "Configurar BD",
            Descripcion = "PostgreSQL en Azure",
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(5),
            ProyectoId = proyecto.Id
        }, ApiTestFactory.JsonOptions);
        var t1 = await t1Resp.Content.ReadFromJsonAsync<TareaDto>(ApiTestFactory.JsonOptions);

        var t2Resp = await client.PostAsJsonAsync("/api/v1/tareas", new CrearTareaDto
        {
            Titulo = "Configurar CI/CD",
            Descripcion = "GitHub Actions",
            Prioridad = PrioridadTarea.Media,
            FechaLimite = DateTime.UtcNow.AddDays(10),
            ProyectoId = proyecto.Id
        }, ApiTestFactory.JsonOptions);
        var t2 = await t2Resp.Content.ReadFromJsonAsync<TareaDto>(ApiTestFactory.JsonOptions);

        Assert.NotNull(t1);
        Assert.NotNull(t2);
        Assert.Equal($"/api/v1/tareas/{t1.Id}", t1Resp.Headers.Location?.AbsolutePath);

        var patchResp = await client.PatchAsJsonAsync(
            $"/api/v1/tareas/{t1.Id}/estado",
            new CambiarEstadoTareaDto { Estado = EstadoTarea.Completada },
            ApiTestFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.NoContent, patchResp.StatusCode);

        var avance = await client.GetFromJsonAsync<AvanceProyectoDto>(
            $"/api/v1/proyectos/{proyecto.Id}/avance",
            ApiTestFactory.JsonOptions);
        Assert.NotNull(avance);
        Assert.Equal(2, avance.TotalTareas);
        Assert.Equal(1, avance.TareasCompletadas);
        Assert.Equal(50m, avance.PorcentajeAvance);

        var putResp = await client.PutAsJsonAsync($"/api/v1/proyectos/{proyecto.Id}", new ActualizarProyectoDto
        {
            Nombre = "Migración Cloud v2",
            Descripcion = "Actualizada",
            FechaInicio = DateTime.UtcNow,
            FechaFinalizacion = DateTime.UtcNow.AddDays(45)
        }, ApiTestFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.NoContent, putResp.StatusCode);

        var lista = await client.GetFromJsonAsync<List<ProyectoDto>>("/api/v1/proyectos", ApiTestFactory.JsonOptions);
        Assert.NotNull(lista);
        Assert.Single(lista);
        Assert.Equal("Migración Cloud v2", lista[0].Nombre);
        Assert.Equal(50m, lista[0].PorcentajeAvance);

        var obtenido = await client.GetFromJsonAsync<ProyectoDto>($"/api/v1/proyectos/{proyecto.Id}", ApiTestFactory.JsonOptions);
        Assert.NotNull(obtenido);
        Assert.Equal(proyecto.Id, obtenido.Id);
        Assert.Equal("Migración Cloud v2", obtenido.Nombre);
    }

    [Fact]
    public async Task EndpointsProyectoInexistente_Retornan404NotFound()
    {
        await using var factory = new ApiTestFactory();
        await factory.InitializeAsync();
        var client = factory.Client;
        var idInexistente = Guid.NewGuid();

        var avanceResp = await client.GetAsync($"/api/v1/proyectos/{idInexistente}/avance");
        var tareasResp = await client.GetAsync($"/api/v1/proyectos/{idInexistente}/tareas");
        var putResp = await client.PutAsJsonAsync($"/api/v1/proyectos/{idInexistente}", new ActualizarProyectoDto
        {
            Nombre = "No existe",
            Descripcion = "Desc",
            FechaInicio = DateTime.UtcNow,
            FechaFinalizacion = DateTime.UtcNow.AddDays(5)
        }, ApiTestFactory.JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, avanceResp.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, tareasResp.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, putResp.StatusCode);
    }

    [Fact]
    public async Task EliminarProyecto_ConTareasPendientes_Retorna409_Y_CuandoCompletadas_Elimina204()
    {
        await using var factory = new ApiTestFactory();
        await factory.InitializeAsync();
        var client = factory.Client;

        var pResp = await client.PostAsJsonAsync("/api/v1/proyectos", new CrearProyectoDto
        {
            Nombre = "Proyecto Protegido",
            Descripcion = "No eliminar con tareas pendientes",
            FechaInicio = DateTime.UtcNow,
            FechaFinalizacion = DateTime.UtcNow.AddDays(10)
        }, ApiTestFactory.JsonOptions);
        var proyecto = await pResp.Content.ReadFromJsonAsync<ProyectoDto>(ApiTestFactory.JsonOptions);
        Assert.NotNull(proyecto);

        var tResp = await client.PostAsJsonAsync("/api/v1/tareas", new CrearTareaDto
        {
            Titulo = "Tarea pendiente",
            Descripcion = "Bloquea borrado",
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(2),
            ProyectoId = proyecto.Id
        }, ApiTestFactory.JsonOptions);
        var tarea = await tResp.Content.ReadFromJsonAsync<TareaDto>(ApiTestFactory.JsonOptions);
        Assert.NotNull(tarea);

        var deleteConflict = await client.DeleteAsync($"/api/v1/proyectos/{proyecto.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteConflict.StatusCode);

        var problem = await deleteConflict.Content.ReadFromJsonAsync<ProblemDetails>(ApiTestFactory.JsonOptions);
        Assert.NotNull(problem);
        Assert.Contains("pendientes", problem.Detail ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        await client.PatchAsJsonAsync(
            $"/api/v1/tareas/{tarea.Id}/estado",
            new CambiarEstadoTareaDto { Estado = EstadoTarea.Completada },
            ApiTestFactory.JsonOptions);

        var deleteOk = await client.DeleteAsync($"/api/v1/proyectos/{proyecto.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteOk.StatusCode);

        var getAfterDelete = await client.GetAsync($"/api/v1/proyectos/{proyecto.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }
}
