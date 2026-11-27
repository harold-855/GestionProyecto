using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Infrastructure.Extensions;
using GestionProyecto.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace GestionProyecto.Infrastructure.IntegrationTests.Extensions;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_SinCadenaDeConexion_LanzaInvalidOperationException()
    {
        var services = new ServiceCollection();
        var configuration = new StubConfiguration(new Dictionary<string, string?>());

        Assert.Throws<InvalidOperationException>(() => services.AddInfrastructure(configuration));
    }

    [Fact]
    public void AddInfrastructure_ConCadenaDeConexion_RegistraDbContextYRepositorios()
    {
        var services = new ServiceCollection();
        var configuration = new StubConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] =
                "Host=localhost;Port=5432;Database=TestDb;Username=postgres;Password=postgres"
        });

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<GestionProyectoDbContext>());
        Assert.NotNull(scope.ServiceProvider.GetService<IProyectoRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<ITareaRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IUnitOfWork>());
    }

    private sealed class StubConfiguration(IDictionary<string, string?> values) : IConfiguration
    {
        public string? this[string key]
        {
            get => values.TryGetValue(key, out var value) ? value : null;
            set => values[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];

        public IChangeToken GetReloadToken() => NullChangeToken.Instance;

        public IConfigurationSection GetSection(string key) => new StubConfigurationSection(this, key);
    }

    private sealed class StubConfigurationSection(IConfiguration root, string path) : IConfigurationSection
    {
        public string? this[string key]
        {
            get => root[$"{path}:{key}"];
            set => root[$"{path}:{key}"] = value;
        }

        public string Key => path.Contains(':') ? path[(path.LastIndexOf(':') + 1)..] : path;
        public string Path => path;
        public string? Value
        {
            get => root[path];
            set => root[path] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => NullChangeToken.Instance;
        public IConfigurationSection GetSection(string key) => new StubConfigurationSection(root, $"{path}:{key}");
    }

    private sealed class NullChangeToken : IChangeToken
    {
        public static readonly NullChangeToken Instance = new();
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => NullDisposable.Instance;
    }

    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();
        public void Dispose() { }
    }
}
