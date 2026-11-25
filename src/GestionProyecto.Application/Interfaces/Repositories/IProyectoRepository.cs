using GestionProyecto.Domain.Entities;

namespace GestionProyecto.Application.Interfaces.Repositories;

public interface IProyectoRepository
{
    // Obtener un proyecto por su identificador.
    Task<Proyecto?> ObtenerPorIdAsync(Guid id);

    // Obtener un proyecto incluyendo sus tareas.
    Task<Proyecto?> ObtenerPorIdConTareasAsync(Guid id);

    // Obtener todos los proyectos.
    Task<IReadOnlyList<Proyecto>> ObtenerTodosAsync();

    // Registrar un proyecto.
    Task AgregarAsync(Proyecto proyecto);

    // Actualizar un proyecto.
    void Actualizar(Proyecto proyecto);

    // Eliminar un proyecto.
    void Eliminar(Proyecto proyecto);
}