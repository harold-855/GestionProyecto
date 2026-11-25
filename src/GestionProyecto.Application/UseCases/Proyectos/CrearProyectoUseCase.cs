using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Domain.Entities;

namespace GestionProyecto.Application.UseCases.Proyectos;

public class CrearProyectoUseCase
{
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearProyectoUseCase(
        IProyectoRepository proyectoRepository,
        IUnitOfWork unitOfWork)
    {
        _proyectoRepository = proyectoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProyectoDto> EjecutarAsync(
        CrearProyectoDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1. Crear la entidad a partir de los datos recibidos.
        var proyecto = new Proyecto(
            dto.Nombre,
            dto.Descripcion,
            dto.FechaInicio,
            dto.FechaFinalizacion);

        // 2. Agregar el proyecto al repositorio.
        await _proyectoRepository.AgregarAsync(proyecto);

        // 3. Guardar los cambios en la base de datos.
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        // 4. Convertir la entidad en un DTO de respuesta.
        var respuesta = new ProyectoDto
        {
            Id = proyecto.Id,
            Nombre = proyecto.Nombre,
            Descripcion = proyecto.Descripcion,
            FechaInicio = proyecto.FechaInicio,
            FechaFinalizacion = proyecto.FechaFinalizacion,
            PorcentajeAvance = 0,
            TotalTareas = 0,
            TareasCompletadas = 0
        };

        return respuesta;
    }
}