using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GestionProyecto.IntegrationTests;

public class ApiIntegrationTests
{
    [Fact]
    public async Task Health_Y_Swagger_Responden200Ok()
    {
        await using var factory = new ApiTestFactory();
        await factory.InitializeAsync();

        var healthResponse = await factory.Client.GetAsync("/health");
        var swaggerResponse = await factory.Client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, swaggerResponse.StatusCode);

        using var swaggerJson = JsonDocument.Parse(await swaggerResponse.Content.ReadAsStringAsync());
        var paths = swaggerJson.RootElement.GetProperty("paths")
            .EnumerateObject()
            .Select(p => p.Name)
            .ToList();

        Assert.NotEmpty(paths);
        foreach (var path in paths)
        {
            Assert.Equal(path.ToLowerInvariant(), path);
            if (path != "/health")
            {
                Assert.StartsWith("/api/v1/", path, StringComparison.Ordinal);
            }
        }

        string[] expectedApiPaths =
        [
            "/api/v1/proyectos",
            "/api/v1/proyectos/{id}",
            "/api/v1/proyectos/{id}/avance",
            "/api/v1/proyectos/{id}/tareas",
            "/api/v1/tareas",
            "/api/v1/tareas/{id}",
            "/api/v1/tareas/estado/{estado}",
            "/api/v1/tareas/prioridad/{prioridad}",
            "/api/v1/tareas/vencidas"
        ];

        foreach (var expectedPath in expectedApiPaths)
        {
            Assert.Contains(expectedPath, paths);
        }
    }

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

        // Crear dos tareas asociadas
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

        // Completar una de las dos tareas => 50% de avance
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

        // Actualizar proyecto
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

        // Intentar eliminar con tarea pendiente -> 409 Conflict
        var deleteConflict = await client.DeleteAsync($"/api/v1/proyectos/{proyecto.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteConflict.StatusCode);

        var problem = await deleteConflict.Content.ReadFromJsonAsync<ProblemDetails>(ApiTestFactory.JsonOptions);
        Assert.NotNull(problem);
        Assert.Contains("pendientes", problem.Detail ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        // Completar tarea y volver a eliminar -> 204 NoContent
        await client.PatchAsJsonAsync(
            $"/api/v1/tareas/{tarea.Id}/estado",
            new CambiarEstadoTareaDto { Estado = EstadoTarea.Completada },
            ApiTestFactory.JsonOptions);

        var deleteOk = await client.DeleteAsync($"/api/v1/proyectos/{proyecto.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteOk.StatusCode);

        var getAfterDelete = await client.GetAsync($"/api/v1/proyectos/{proyecto.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Tareas_Filtros_PorProyecto_Estado_Prioridad_Y_Vencidas()
    {
        await using var factory = new ApiTestFactory();
        await factory.InitializeAsync();
        var client = factory.Client;

        var p1Resp = await client.PostAsJsonAsync("/api/v1/proyectos", new CrearProyectoDto
        {
            Nombre = "P1",
            Descripcion = "D1",
            FechaInicio = DateTime.UtcNow.AddDays(-10),
            FechaFinalizacion = DateTime.UtcNow.AddDays(20)
        }, ApiTestFactory.JsonOptions);
        var p1 = await p1Resp.Content.ReadFromJsonAsync<ProyectoDto>(ApiTestFactory.JsonOptions);

        var p2Resp = await client.PostAsJsonAsync("/api/v1/proyectos", new CrearProyectoDto
        {
            Nombre = "P2",
            Descripcion = "D2",
            FechaInicio = DateTime.UtcNow.AddDays(-10),
            FechaFinalizacion = DateTime.UtcNow.AddDays(20)
        }, ApiTestFactory.JsonOptions);
        var p2 = await p2Resp.Content.ReadFromJsonAsync<ProyectoDto>(ApiTestFactory.JsonOptions);

        Assert.NotNull(p1);
        Assert.NotNull(p2);

        // Tarea 1: P1, Alta, Pendiente, Vencida
        var t1 = await (await client.PostAsJsonAsync("/api/v1/tareas", new CrearTareaDto
        {
            Titulo = "Urgente vencida",
            Descripcion = "D",
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(-2),
            ProyectoId = p1.Id
        }, ApiTestFactory.JsonOptions)).Content.ReadFromJsonAsync<TareaDto>(ApiTestFactory.JsonOptions);

        // Tarea 2: P1, Media, EnProgreso, Futura
        var t2 = await (await client.PostAsJsonAsync("/api/v1/tareas", new CrearTareaDto
        {
            Titulo = "En progreso",
            Descripcion = "D",
            Prioridad = PrioridadTarea.Media,
            FechaLimite = DateTime.UtcNow.AddDays(5),
            ProyectoId = p1.Id
        }, ApiTestFactory.JsonOptions)).Content.ReadFromJsonAsync<TareaDto>(ApiTestFactory.JsonOptions);
        await client.PatchAsJsonAsync($"/api/v1/tareas/{t2!.Id}/estado",
            new CambiarEstadoTareaDto { Estado = EstadoTarea.EnProgreso }, ApiTestFactory.JsonOptions);

        // Tarea 3: P2, Alta, Completada, Fecha pasada (NO debe salir en vencidas)
        var t3 = await (await client.PostAsJsonAsync("/api/v1/tareas", new CrearTareaDto
        {
            Titulo = "Ya completada",
            Descripcion = "D",
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(-5),
            ProyectoId = p2.Id
        }, ApiTestFactory.JsonOptions)).Content.ReadFromJsonAsync<TareaDto>(ApiTestFactory.JsonOptions);
        await client.PatchAsJsonAsync($"/api/v1/tareas/{t3!.Id}/estado",
            new CambiarEstadoTareaDto { Estado = EstadoTarea.Completada }, ApiTestFactory.JsonOptions);

        // Consultar tareas de P1
        var tareasP1 = await client.GetFromJsonAsync<List<TareaDto>>(
            $"/api/v1/proyectos/{p1.Id}/tareas", ApiTestFactory.JsonOptions);
        Assert.NotNull(tareasP1);
        Assert.Equal(2, tareasP1.Count);

        // Filtro combinado estado=Pendiente & prioridad=Alta
        var combinadas = await client.GetFromJsonAsync<List<TareaDto>>(
            "/api/v1/tareas?estado=Pendiente&prioridad=Alta", ApiTestFactory.JsonOptions);
        Assert.NotNull(combinadas);
        var unica = Assert.Single(combinadas);
        Assert.Equal(t1!.Id, unica.Id);

        // Filtro por estado y prioridad por separado
        var enProgreso = await client.GetFromJsonAsync<List<TareaDto>>(
            "/api/v1/tareas/estado/EnProgreso", ApiTestFactory.JsonOptions);
        Assert.Single(enProgreso!);

        var media = await client.GetFromJsonAsync<List<TareaDto>>(
            "/api/v1/tareas/prioridad/Media", ApiTestFactory.JsonOptions);
        Assert.Single(media!);

        // Consultar vencidas no completadas
        var vencidas = await client.GetFromJsonAsync<List<TareaDto>>(
            "/api/v1/tareas/vencidas", ApiTestFactory.JsonOptions);
        Assert.NotNull(vencidas);
        var vencida = Assert.Single(vencidas);
        Assert.Equal(t1.Id, vencida.Id);
        Assert.True(vencida.EstaVencida);

        // Actualizar y eliminar una tarea
        var putTarea = await client.PutAsJsonAsync($"/api/v1/tareas/{t2.Id}", new ActualizarTareaDto
        {
            Titulo = "En progreso editada",
            Descripcion = "Editada",
            Prioridad = PrioridadTarea.Baja,
            FechaLimite = DateTime.UtcNow.AddDays(8)
        }, ApiTestFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.NoContent, putTarea.StatusCode);

        var delTarea = await client.DeleteAsync($"/api/v1/tareas/{t2.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delTarea.StatusCode);
    }

    [Fact]
    public async Task Validaciones_Y_Excepciones_RetornanProblemDetails()
    {
        await using var factory = new ApiTestFactory();
        await factory.InitializeAsync();
        var client = factory.Client;

        // Fecha finalización anterior a inicio -> 400 BadRequest
        var badDatesResp = await client.PostAsJsonAsync("/api/v1/proyectos", new CrearProyectoDto
        {
            Nombre = "Fechas mal",
            Descripcion = "Desc",
            FechaInicio = DateTime.UtcNow.AddDays(5),
            FechaFinalizacion = DateTime.UtcNow
        }, ApiTestFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, badDatesResp.StatusCode);

        // Campos requeridos vacíos -> 400 BadRequest
        var badModelResp = await client.PostAsJsonAsync("/api/v1/proyectos", new CrearProyectoDto
        {
            Nombre = "",
            Descripcion = "",
            FechaInicio = DateTime.UtcNow,
            FechaFinalizacion = DateTime.UtcNow.AddDays(1)
        }, ApiTestFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, badModelResp.StatusCode);

        // Crear tarea para proyecto inexistente -> 404 NotFound
        var missingProjectTaskResp = await client.PostAsJsonAsync("/api/v1/tareas", new CrearTareaDto
        {
            Titulo = "Huérfana",
            Descripcion = "Desc",
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(1),
            ProyectoId = Guid.NewGuid()
        }, ApiTestFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.NotFound, missingProjectTaskResp.StatusCode);

        // Consultar proyecto y tarea inexistentes -> 404 NotFound
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/v1/proyectos/{Guid.NewGuid()}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/v1/tareas/{Guid.NewGuid()}")).StatusCode);
    }
}
