using GestionProyecto.Application.DTOs.Tareas;
using GestionProyecto.Application.Exceptions;
using GestionProyecto.Application.Interfaces;
using GestionProyecto.Application.Interfaces.Repositories;
using GestionProyecto.Domain.Entities;

namespace GestionProyecto.Application.UseCases.Tareas;

public class CrearTareaUseCase
{
    private readonly ITareaRepository _tareaRepository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearTareaUseCase(
        ITareaRepository tareaRepository,
        IProyectoRepository proyectoRepository,
        IUnitOfWork unitOfWork)
    {
        _tareaRepository = tareaRepository;
        _proyectoRepository = proyectoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TareaDto> EjecutarAsync(
        CrearTareaDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1. Comprobar que el proyecto existe.
        var proyecto = await _proyectoRepository
            .ObtenerPorIdAsync(dto.ProyectoId);

        if (proyecto is null)
        {
            throw new NotFoundException(
                "No se puede crear la tarea porque el proyecto no existe.");
        }

        // 2. Crear la entidad Tarea.
        var tarea = new Tarea(
            dto.Titulo,
            dto.Descripcion,
            dto.Prioridad,
            dto.FechaLimite,
            dto.ProyectoId);

        // 3. Agregar la tarea al repositorio.
        await _tareaRepository.AgregarAsync(tarea);

        // 4. Guardar los cambios.
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        // 5. Construir el DTO de respuesta.
        var respuesta = new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Estado = tarea.Estado,
            Prioridad = tarea.Prioridad,
            FechaLimite = tarea.FechaLimite,
            ProyectoId = tarea.ProyectoId,
            EstaVencida = tarea.EstaVencida()
        };

        return respuesta;
    }
}