using GestionProyecto.Domain.Entities;
using GestionProyecto.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace GestionProyecto.Infrastructure.IntegrationTests.Persistence;

public class ModelConfigurationTests
{
    [Fact]
    public void ProyectoConfiguration_DefineTablaRestriccionesYRelacionCascada()
    {
        using var context = DbContextTestFactory.Create();
        var entityType = context.Model.FindEntityType(typeof(Proyecto));

        Assert.NotNull(entityType);
        Assert.Equal("Proyectos", entityType.GetTableName());
        Assert.Equal(150, entityType.FindProperty(nameof(Proyecto.Nombre))?.GetMaxLength());
        Assert.Equal(500, entityType.FindProperty(nameof(Proyecto.Descripcion))?.GetMaxLength());
        Assert.False(entityType.FindProperty(nameof(Proyecto.Nombre))?.IsNullable);
        Assert.False(entityType.FindProperty(nameof(Proyecto.Descripcion))?.IsNullable);

        var navigation = entityType.FindNavigation(nameof(Proyecto.Tareas));
        Assert.NotNull(navigation);
        Assert.Equal(PropertyAccessMode.Field, navigation.GetPropertyAccessMode());
        Assert.Equal(DeleteBehavior.Cascade, navigation.ForeignKey.DeleteBehavior);
    }

    [Fact]
    public void TareaConfiguration_DefineTablaRestriccionesYClaveForanea()
    {
        using var context = DbContextTestFactory.Create();
        var entityType = context.Model.FindEntityType(typeof(Tarea));

        Assert.NotNull(entityType);
        Assert.Equal("Tareas", entityType.GetTableName());
        Assert.Equal(150, entityType.FindProperty(nameof(Tarea.Titulo))?.GetMaxLength());
        Assert.Equal(500, entityType.FindProperty(nameof(Tarea.Descripcion))?.GetMaxLength());
        Assert.False(entityType.FindProperty(nameof(Tarea.Titulo))?.IsNullable);
        Assert.False(entityType.FindProperty(nameof(Tarea.Descripcion))?.IsNullable);

        var fk = Assert.Single(entityType.GetForeignKeys());
        Assert.Equal(typeof(Proyecto), fk.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }
}
