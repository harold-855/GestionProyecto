using System.Reflection;
using System.Text.RegularExpressions;
using GestionProyecto.Api.Controllers;
using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.ArchitectureTests.Helpers;
using GestionProyecto.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GestionProyecto.ArchitectureTests.Conventions;

public partial class ApiConventionsTests
{
    [GeneratedRegex(@"\{[^}]+\}")]
    private static partial Regex RouteParameterRegex();

    [Fact]
    public void Controladores_DebenHeredarDeApiControllerBase()
    {
        var controllers = ArchitectureAssemblies.GetConcreteClassesInNamespace(
            ArchitectureAssemblies.Api,
            "GestionProyecto.Api.Controllers");

        Assert.NotEmpty(controllers);
        foreach (var controller in controllers)
        {
            Assert.EndsWith("Controller", controller.Name, StringComparison.Ordinal);
            Assert.True(
                typeof(ApiControllerBase).IsAssignableFrom(controller),
                $"El controlador '{controller.Name}' debe heredar de ApiControllerBase.");
        }
    }

    [Fact]
    public void ApiControllerBase_DebeDefinirRutaVersionadaV1()
    {
        var routeAttribute = typeof(ApiControllerBase).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(routeAttribute);
        Assert.Equal("api/v1/[controller]", routeAttribute.Template);
    }

    [Fact]
    public void AccionesHttp_DebenUsarPlantillasDeRutaEnMinusculas()
    {
        var controllers = ArchitectureAssemblies.Api
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ApiControllerBase).IsAssignableFrom(t))
            .ToList();

        foreach (var controller in controllers)
        {
            var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                var httpAttributes = method.GetCustomAttributes<HttpMethodAttribute>();
                foreach (var attr in httpAttributes)
                {
                    if (string.IsNullOrEmpty(attr.Template))
                        continue;

                    var literalSegments = RouteParameterRegex().Replace(attr.Template, string.Empty);
                    Assert.Equal(literalSegments.ToLowerInvariant(), literalSegments);
                }
            }
        }
    }

    [Fact]
    public void Controladores_NoDebenDependerDirectamenteDeRepositoriosODbContext()
    {
        Type[] forbiddenDependencies =
        [
            typeof(IProyectoRepository),
            typeof(ITareaRepository),
            typeof(IUnitOfWork),
            typeof(GestionProyectoDbContext)
        ];

        var controllers = ArchitectureAssemblies.GetConcreteClassesInNamespace(
            ArchitectureAssemblies.Api,
            "GestionProyecto.Api.Controllers");

        foreach (var controller in controllers)
        {
            var ctorParameterTypes = controller
                .GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType);

            foreach (var paramType in ctorParameterTypes)
            {
                Assert.DoesNotContain(paramType, forbiddenDependencies);
            }
        }
    }
}
