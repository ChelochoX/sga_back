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
    public async Task<IEnumerable<PermisoDto>> ObtenerPermisosPorRol(int idRol)
    {
        return await _repository.ObtenerPermisosPorRol(idRol);
    }

    public async Task<IEnumerable<EntidadConRecursosDto>> ObtenerEntidadesConRecursos()
    {
        return await _repository.ObtenerEntidadesConRecursos();
    }

    public async Task AsignarPermisosARol(int idRol, List<(int idEntidad, int idRecurso)> permisos)
    {
        await _repository.AsignarPermisosARol(idRol, permisos);
    }
}
