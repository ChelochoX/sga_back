using sga_back.DTOs;
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
    public async Task<IEnumerable<RolDto>> ObtenerRoles()
    {
        return await _repository.ObtenerRoles();
    }
    public async Task<IEnumerable<RecursoDto>> ObtenerRecursos()
    {
        return await _repository.ObtenerRecursos();
    }
    public async Task<IEnumerable<EntidadDto>> ObtenerEntidades()
    {
        return await _repository.ObtenerEntidades();
    }
    public async Task<IEnumerable<PermisoDto>> ObtenerPermisosPorRol(int idRol)
    {
        return await _repository.ObtenerPermisosPorRol(idRol);
    }
}
