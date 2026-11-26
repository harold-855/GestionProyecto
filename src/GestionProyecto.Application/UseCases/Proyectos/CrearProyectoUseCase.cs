using GestionProyecto.Application.DTOs.Proyectos;
using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Application.Mappings;
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
        ArgumentNullException.ThrowIfNull(dto);

        var proyecto = new Proyecto(
            dto.Nombre,
            dto.Descripcion,
            dto.FechaInicio,
            dto.FechaFinalizacion);

        await _proyectoRepository.AgregarAsync(proyecto);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return proyecto.ToDto();
    }
}