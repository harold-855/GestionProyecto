using GestionProyecto.ArchitectureTests.Helpers;

namespace GestionProyecto.ArchitectureTests.Layers;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_NoDebeReferenciarOtrasCapasNiFrameworksExternos()
    {
        ArchitectureAssemblies.AssertDoesNotReference(
            ArchitectureAssemblies.Domain,
            "GestionProyecto.Application",
            "GestionProyecto.Infrastructure",
            "GestionProyecto.Api",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore");
    }

    [Fact]
    public void Application_NoDebeReferenciarInfrastructureNiApi()
    {
        ArchitectureAssemblies.AssertDoesNotReference(
            ArchitectureAssemblies.Application,
            "GestionProyecto.Infrastructure",
            "GestionProyecto.Api",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore");
    }

    [Fact]
    public void Infrastructure_NoDebeReferenciarApi()
    {
        ArchitectureAssemblies.AssertDoesNotReference(
            ArchitectureAssemblies.Infrastructure,
            "GestionProyecto.Api");
    }

    [Fact]
    public void Api_DebeReferenciarApplicationEInfrastructure()
    {
        var referencedNames = ArchitectureAssemblies.Api
            .GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("GestionProyecto.Application", referencedNames);
        Assert.Contains("GestionProyecto.Infrastructure", referencedNames);
    }
}
