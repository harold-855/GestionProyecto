using System.Net;
using System.Net.Http.Json;
using GestionProyecto.Api.IntegrationTests.Fixtures;
using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Domain.Enums;

namespace GestionProyecto.Api.IntegrationTests.Endpoints;

public class TareasEndpointsTests
{
    [Fact]
    public async Task Tareas_Filtros_PorProyecto_Estado_Prioridad_Y_Vencidas()
    {
        await using var factory = new ApiTestFactory();
        await factory.InitializeAsync();
        var client = factory.Client;

        var p1 = await (await client.PostAsJsonAsync("/api/v1/proyectos", new CrearProyectoDto
        {
            Nombre = "P1",
            Descripcion = "D1",
            FechaInicio = DateTime.UtcNow.AddDays(-10),
            FechaFinalizacion = DateTime.UtcNow.AddDays(20)
        }, ApiTestFactory.JsonOptions)).Content.ReadFromJsonAsync<ProyectoDto>(ApiTestFactory.JsonOptions);

        var p2 = await (await client.PostAsJsonAsync("/api/v1/proyectos", new CrearProyectoDto
        {
            Nombre = "P2",
            Descripcion = "D2",
            FechaInicio = DateTime.UtcNow.AddDays(-10),
            FechaFinalizacion = DateTime.UtcNow.AddDays(20)
        }, ApiTestFactory.JsonOptions)).Content.ReadFromJsonAsync<ProyectoDto>(ApiTestFactory.JsonOptions);

        Assert.NotNull(p1);
        Assert.NotNull(p2);

        var t1 = await (await client.PostAsJsonAsync("/api/v1/tareas", new CrearTareaDto
        {
            Titulo = "Urgente vencida",
            Descripcion = "D",
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(-2),
            ProyectoId = p1.Id
        }, ApiTestFactory.JsonOptions)).Content.ReadFromJsonAsync<TareaDto>(ApiTestFactory.JsonOptions);

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

        var tareasP1 = await client.GetFromJsonAsync<List<TareaDto>>(
            $"/api/v1/proyectos/{p1.Id}/tareas", ApiTestFactory.JsonOptions);
        Assert.NotNull(tareasP1);
        Assert.Equal(2, tareasP1.Count);

        var combinadas = await client.GetFromJsonAsync<List<TareaDto>>(
            "/api/v1/tareas?estado=Pendiente&prioridad=Alta", ApiTestFactory.JsonOptions);
        Assert.NotNull(combinadas);
        var unica = Assert.Single(combinadas);
        Assert.Equal(t1!.Id, unica.Id);

        var enProgreso = await client.GetFromJsonAsync<List<TareaDto>>(
            "/api/v1/tareas/estado/EnProgreso", ApiTestFactory.JsonOptions);
        Assert.Single(enProgreso!);

        var media = await client.GetFromJsonAsync<List<TareaDto>>(
            "/api/v1/tareas/prioridad/Media", ApiTestFactory.JsonOptions);
        Assert.Single(media!);

        var vencidas = await client.GetFromJsonAsync<List<TareaDto>>(
            "/api/v1/tareas/vencidas", ApiTestFactory.JsonOptions);
        Assert.NotNull(vencidas);
        var vencida = Assert.Single(vencidas);
        Assert.Equal(t1.Id, vencida.Id);
        Assert.True(vencida.EstaVencida);

        var tareaObtenida = await client.GetFromJsonAsync<TareaDto>($"/api/v1/tareas/{t1.Id}", ApiTestFactory.JsonOptions);
        Assert.NotNull(tareaObtenida);
        Assert.Equal(t1.Id, tareaObtenida.Id);

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
    public async Task EndpointsTareaInexistente_Retornan404NotFound()
    {
        await using var factory = new ApiTestFactory();
        await factory.InitializeAsync();
        var client = factory.Client;
        var idInexistente = Guid.NewGuid();

        var putResp = await client.PutAsJsonAsync($"/api/v1/tareas/{idInexistente}", new ActualizarTareaDto
        {
            Titulo = "No existe",
            Descripcion = "Desc",
            Prioridad = PrioridadTarea.Baja,
            FechaLimite = DateTime.UtcNow.AddDays(3)
        }, ApiTestFactory.JsonOptions);
        var patchResp = await client.PatchAsJsonAsync($"/api/v1/tareas/{idInexistente}/estado",
            new CambiarEstadoTareaDto { Estado = EstadoTarea.Completada }, ApiTestFactory.JsonOptions);
        var deleteResp = await client.DeleteAsync($"/api/v1/tareas/{idInexistente}");

        Assert.Equal(HttpStatusCode.NotFound, putResp.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, patchResp.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResp.StatusCode);
    }
}
