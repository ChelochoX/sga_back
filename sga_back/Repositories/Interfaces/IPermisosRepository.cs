using sga_back.DTOs;

namespace sga_back.Repositories.Interfaces;

public interface IPermisosRepository
{
    Task<bool> TienePermiso(int idUsuario, string entidad, string recurso);
    Task<IEnumerable<RolDto>> ObtenerRoles();
    Task<IEnumerable<RecursoDto>> ObtenerRecursos();
    Task<IEnumerable<EntidadDto>> ObtenerEntidades();
    Task<IEnumerable<PermisoDto>> ObtenerPermisosPorRol(int idRol);
}
