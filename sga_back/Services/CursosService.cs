using AutoMapper;
using FluentValidation;
using sga_back.DTOs;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class CursosService : ICursosService
{
    private readonly ICursosRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CursosService> _logger;
    private readonly IValidator<CursoRequest> _validator;
    private readonly IValidator<ObtenerCursosRequest> _validatorFecha;

    public CursosService(ILogger<CursosService> logger, ICursosRepository repository, IMapper mapper, IValidator<CursoRequest> validator, IValidator<ObtenerCursosRequest> validatorFecha)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
        _validatorFecha = validatorFecha;
    }

    public async Task<int> Insertar(CursoRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        _logger.LogInformation("Insertando curso: {Nombre}", request.Nombre);

        var curso = _mapper.Map<Curso>(request);
        curso.Conceptos = request.Conceptos != null
            ? _mapper.Map<List<CursoConcepto>>(request.Conceptos)
            : new List<CursoConcepto>();

        for (int i = 0; i < curso.Conceptos.Count; i++)
        {
            curso.Conceptos[i].Vencimientos = request.Conceptos[i].Vencimientos != null
                ? _mapper.Map<List<CursoConceptoVencimiento>>(request.Conceptos[i].Vencimientos)
                : new List<CursoConceptoVencimiento>();
        }

        int id = await _repository.Insertar(curso);

        _logger.LogInformation("Curso insertado exitosamente con ID: {IdCurso}", id);
        return id;
    }

    public async Task<int> Actualizar(int id, CursoRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var curso = _mapper.Map<Curso>(request);
        curso.IdCurso = id;
        curso.Conceptos = request.Conceptos != null
            ? _mapper.Map<List<CursoConcepto>>(request.Conceptos)
            : new List<CursoConcepto>();

        for (int i = 0; i < curso.Conceptos.Count; i++)
        {
            curso.Conceptos[i].Vencimientos = request.Conceptos[i].Vencimientos != null
                ? _mapper.Map<List<CursoConceptoVencimiento>>(request.Conceptos[i].Vencimientos)
                : new List<CursoConceptoVencimiento>();
        }

        int filasAfectadas = await _repository.Actualizar(curso);

        if (filasAfectadas == 0)
        {
            _logger.LogWarning("No se encontró el curso con ID: {IdCurso} para actualizar.", id);
            throw new InvalidOperationException("No se encontró el curso para actualizar.");
        }

        _logger.LogInformation("Curso con ID: {IdCurso} actualizado exitosamente.", id);
        return filasAfectadas;
    }

    public async Task<bool> Eliminar(int id)
    {
        bool eliminado = await _repository.Eliminar(id);

        if (!eliminado)
        {
            _logger.LogWarning("No se encontró el curso con ID: {IdCurso} para eliminar.", id);
            throw new InvalidOperationException("No se encontró el curso para eliminar.");
        }

        _logger.LogInformation("Curso con ID: {IdCurso} eliminado exitosamente.", id);
        return eliminado;
    }

    public async Task<CursoDetalleDto?> ObtenerDetallePorId(int idCurso)
    {
        return await _repository.ObtenerDetallePorId(idCurso);
    }

    public async Task<IEnumerable<CursoDto>> ObtenerCursosPorFecha(ObtenerCursosRequest request)
    {
        var validationResult = await _validatorFecha.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        _logger.LogInformation("Llamando a repositorio para obtener cursos por fechas...");
        return await _repository.ObtenerCursosPorFecha(request);
    }

    public async Task CambiarEstado(int idCurso, bool activo)
    {
        await _repository.CambiarEstado(idCurso, activo);
    }
}

