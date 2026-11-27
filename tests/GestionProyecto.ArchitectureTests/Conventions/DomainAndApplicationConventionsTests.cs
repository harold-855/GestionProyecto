using GestionProyecto.ArchitectureTests.Helpers;
using GestionProyecto.Domain.Shared;

namespace GestionProyecto.ArchitectureTests.Conventions;

public class DomainAndApplicationConventionsTests
{
    [Fact]
    public void EntidadesDeDominio_DebenHeredarDeBaseEntity()
    {
        var entityTypes = ArchitectureAssemblies.GetConcreteClassesInNamespace(
            ArchitectureAssemblies.Domain,
            "GestionProyecto.Domain.Entities");

        Assert.NotEmpty(entityTypes);
        foreach (var entityType in entityTypes)
        {
            Assert.True(
                typeof(BaseEntity).IsAssignableFrom(entityType),
                $"La entidad '{entityType.Name}' debe heredar de BaseEntity.");
        }
    }

    [Fact]
    public void EntidadesDeDominio_NoDebenExponerSettersPublicos()
    {
        var entityTypes = ArchitectureAssemblies.GetConcreteClassesInNamespace(
            ArchitectureAssemblies.Domain,
            "GestionProyecto.Domain.Entities");

        Assert.NotEmpty(entityTypes);
        foreach (var entityType in entityTypes)
        {
            var publicSetters = entityType
                .GetProperties()
                .Where(p => p.SetMethod is { IsPublic: true })
                .Select(p => p.Name)
                .ToList();

            Assert.Empty(publicSetters);
        }
    }

    [Fact]
    public void CasosDeUso_DebenSeguirConvencionDeNombreYNamespace()
    {
        var useCaseTypes = ArchitectureAssemblies.GetConcreteClassesInNamespace(
            ArchitectureAssemblies.Application,
            "GestionProyecto.Application.UseCases",
            includeSubNamespaces: true);

        Assert.NotEmpty(useCaseTypes);
        foreach (var type in useCaseTypes)
        {
            Assert.EndsWith("UseCase", type.Name, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Repositorios_InterfacesEImplementacionesDebenCumplirConvenciones()
    {
        var repoInterfaces = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t => t.IsInterface &&
                        t.Namespace == "GestionProyecto.Application.Interfaces.Repositories")
            .ToList();

        var repoImplementations = ArchitectureAssemblies.GetConcreteClassesInNamespace(
                ArchitectureAssemblies.Infrastructure,
                "GestionProyecto.Infrastructure.Repositories")
            .Where(t => t.Name.EndsWith("Repository", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(repoInterfaces);
        foreach (var iface in repoInterfaces)
        {
            Assert.StartsWith("I", iface.Name, StringComparison.Ordinal);
            Assert.EndsWith("Repository", iface.Name, StringComparison.Ordinal);
            Assert.Contains(repoImplementations, impl => iface.IsAssignableFrom(impl));
        }
    }
}
