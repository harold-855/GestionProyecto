using System.Reflection;
using System.Runtime.CompilerServices;
using GestionProyecto.Api.Controllers;
using GestionProyecto.Application.Interfaces;
using GestionProyecto.Domain.Shared;
using GestionProyecto.Infrastructure.Persistence;

namespace GestionProyecto.ArchitectureTests.Helpers;

/// <summary>
/// Centraliza la carga de ensamblados y utilidades de verificación de dependencias.
/// </summary>
internal static class ArchitectureAssemblies
{
    public static Assembly Domain => typeof(BaseEntity).Assembly;
    public static Assembly Application => typeof(IUnitOfWork).Assembly;
    public static Assembly Infrastructure => typeof(GestionProyectoDbContext).Assembly;
    public static Assembly Api => typeof(ApiControllerBase).Assembly;

    public static IReadOnlyList<Type> GetConcreteClassesInNamespace(
        Assembly assembly,
        string namespaceName,
        bool includeSubNamespaces = false)
    {
        return assembly
            .GetTypes()
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        !t.IsNested &&
                        t.GetCustomAttribute<CompilerGeneratedAttribute>() is null &&
                        t.Namespace is not null &&
                        (includeSubNamespaces
                            ? t.Namespace.StartsWith(namespaceName, StringComparison.Ordinal)
                            : t.Namespace == namespaceName))
            .ToList();
    }

    public static void AssertDoesNotReference(
        Assembly source,
        params string[] forbiddenAssemblyPrefixes)
    {
        var referencedNames = source
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToArray();

        foreach (var prefix in forbiddenAssemblyPrefixes)
        {
            var match = referencedNames.FirstOrDefault(
                name => name.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith($"{prefix}.", StringComparison.OrdinalIgnoreCase));

            Assert.True(
                match is null,
                $"El ensamblado '{source.GetName().Name}' no debe referenciar '{match}'.");
        }
    }
}
