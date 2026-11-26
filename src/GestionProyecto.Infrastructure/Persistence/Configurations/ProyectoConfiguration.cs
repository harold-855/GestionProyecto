using GestionProyecto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionProyecto.Infrastructure.Persistence.Configurations;

public class ProyectoConfiguration : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        // Nombre de la tabla.
        builder.ToTable("Proyectos");

        // Clave primaria.
        builder.HasKey(p => p.Id);

        // Configuración del identificador.
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        // Configuración del nombre.
        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        // Configuración de la descripción.
        builder.Property(p => p.Descripcion)
            .IsRequired()
            .HasMaxLength(500);

        // Configuración de las fechas.
        builder.Property(p => p.FechaInicio)
            .IsRequired();

        builder.Property(p => p.FechaFinalizacion)
            .IsRequired();

        // Relación uno a muchos con Tarea.
        builder.HasMany(p => p.Tareas)
            .WithOne(t => t.Proyecto)
            .HasForeignKey(t => t.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Acceso a la colección privada de tareas.
        builder.Navigation(p => p.Tareas)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}