using System.Net;
using System.Text.Json;
using GestionProyecto.Api.IntegrationTests.Fixtures;

namespace GestionProyecto.Api.IntegrationTests.Endpoints;

public class SwaggerAndHealthTests
{
    [Fact]
    public async Task Health_Y_Swagger_Responden200Ok_Y_RutasVersionadasEnMinusculas()
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
}
