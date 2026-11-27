using System.Text.Json;
using GestionProyecto.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace GestionProyecto.Api.IntegrationTests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task ExcepcionNoControlada_EnProduccion_Retorna500SinExponerDetalles()
    {
        var problem = await ExecuteMiddlewareWithUnhandledExceptionAsync(Environments.Production);

        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("Ocurrió un error inesperado al procesar la solicitud.", problem.Detail);
    }

    [Fact]
    public async Task ExcepcionNoControlada_EnDesarrollo_Retorna500ConDetalleDeExcepcion()
    {
        var problem = await ExecuteMiddlewareWithUnhandledExceptionAsync(Environments.Development);

        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("Fallo interno de prueba", problem.Detail);
    }

    private static async Task<ProblemDetails> ExecuteMiddlewareWithUnhandledExceptionAsync(string environmentName)
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidProgramException("Fallo interno de prueba"),
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            new StubHostEnvironment(environmentName));

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/proyectos";
        await using var bodyStream = new MemoryStream();
        context.Response.Body = bodyStream;

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        bodyStream.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(bodyStream);
        Assert.NotNull(problem);
        return problem;
    }

    private sealed class StubHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "GestionProyecto.Api";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
