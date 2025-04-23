using sga_back.Repositories.Interfaces;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class PermisosService : IPermisosService
{
    private readonly IPermisosRepository _repository;

    public PermisosService(IPermisosRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> TienePermiso(int idUsuario, string entidad, string recurso)
    {
        return await _repository.TienePermiso(idUsuario, entidad, recurso);
    }
}
