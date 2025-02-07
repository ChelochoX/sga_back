using AutoMapper;
using sga_back.Common;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using sga_back.Response;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class InscripcionesService : IInscripcionesService
{
    private readonly IInscripcionesRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<InscripcionesService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public InscripcionesService(IInscripcionesRepository repository, IMapper mapper, ILogger<InscripcionesService> logger, IServiceProvider serviceProvider)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<int> Insertar(InscripcionRequest request)
    {
        await ValidationHelper.ValidarAsync(request, _serviceProvider);

        Inscripcion inscripcion = _mapper.Map<Inscripcion>(request);
        return await _repository.Insertar(inscripcion);
    }

    public async Task<int> Actualizar(int idInscripcion, InscripcionRequest request)
    {
        Inscripcion inscripcion = _mapper.Map<Inscripcion>(request);
        inscripcion.IdInscripcion = idInscripcion;

        return await _repository.Actualizar(inscripcion);
    }

    public async Task<bool> Eliminar(int idInscripcion)
    {
        return await _repository.Eliminar(idInscripcion);
    }

    public async Task<InscripcionResponse?> ObtenerPorId(int idInscripcion)
    {
        var inscripcion = await _repository.ObtenerPorId(idInscripcion);
        return inscripcion != null ? _mapper.Map<InscripcionResponse>(inscripcion) : null;
    }

    public async Task<IEnumerable<InscripcionResponse>> ObtenerTodas()
    {
        var inscripciones = await _repository.ObtenerTodas();
        return _mapper.Map<IEnumerable<InscripcionResponse>>(inscripciones);
    }
}
