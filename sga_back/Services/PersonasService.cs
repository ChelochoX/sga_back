using AutoMapper;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class PersonasService : IPersonasService
{
    private readonly IPersonasRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<PersonasService> _logger;

    public PersonasService(ILogger<PersonasService> logger, IPersonasRepository repository, IMapper mapper)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<int> Insertar(PersonaRequest request)
    {
        _logger.LogInformation("Insertando persona con cédula: {Cedula}", request.Cedula);

        // Mapeo de request a modelo
        var persona = _mapper.Map<Persona>(request);
        persona.FechaRegistro = DateTime.UtcNow;

        // Llamar al repositorio para insertar la persona
        var id = await _repository.Insertar(persona);

        // Si el repositorio retorna 0, significa que la cédula ya está registrada
        if (id == 0)
        {
            _logger.LogWarning("No se pudo insertar la persona. La cédula {Cedula} ya existe.", request.Cedula);
            throw new InvalidOperationException("La cédula ya está registrada.");
        }

        _logger.LogInformation("Persona insertada exitosamente con ID: {Id}", id);
        return id;
    }

    public async Task<int> Actualizar(int id, PersonaRequest request)
    {
        var persona = _mapper.Map<Persona>(request);
        persona.IdPersona = id;

        int filasAfectadas = await _repository.Actualizar(persona);
        if (filasAfectadas == 0)
        {
            throw new InvalidOperationException("No se encontró la persona para actualizar.");
        }

        return filasAfectadas;
    }

    public async Task<bool> Eliminar(int id)
    {
        bool eliminado = await _repository.Eliminar(id);
        if (!eliminado)
        {
            throw new InvalidOperationException("No se encontró la persona para eliminar.");
        }

        return eliminado;
    }
}
