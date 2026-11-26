using GestionProyecto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionProyecto.Infrastructure.Persistence.Configurations;

public class TareaConfiguration : IEntityTypeConfiguration<Tarea>
{
    public void Configure(EntityTypeBuilder<Tarea> builder)
    {
        // Nombre de la tabla.
        builder.ToTable("Tareas");

        // Clave primaria.
        builder.HasKey(t => t.Id);

        // Configuración del identificador.
        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        // Configuración del título.
        builder.Property(t => t.Titulo)
            .IsRequired()
            .HasMaxLength(150);

        // Configuración de la descripción.
        builder.Property(t => t.Descripcion)
            .IsRequired()
            .HasMaxLength(500);

        // Configuración del estado.
        builder.Property(t => t.Estado)
            .IsRequired()
            .HasConversion<int>();

        // Configuración de la prioridad.
        builder.Property(t => t.Prioridad)
            .IsRequired()
            .HasConversion<int>();

        // Configuración de la fecha límite.
        builder.Property(t => t.FechaLimite)
            .IsRequired();

        // Configuración de la clave foránea.
        builder.Property(t => t.ProyectoId)
            .IsRequired();

        // Relación con Proyecto.
        builder.HasOne(t => t.Proyecto)
            .WithMany(p => p.Tareas)
            .HasForeignKey(t => t.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}