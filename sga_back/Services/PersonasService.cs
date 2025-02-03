using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class PersonasService : IPersonasService
{
    private readonly IPersonasRepository _repository;
    private readonly ILogger<PersonasService> _logger;

    public PersonasService(ILogger<PersonasService> logger, IPersonasRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<int> Insertar(Persona persona)
    {
        return await _repository.Insertar(persona);
    }
}
