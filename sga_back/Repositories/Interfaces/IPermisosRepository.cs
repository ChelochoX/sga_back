using sga_back.DTOs;

namespace sga_back.Repositories.Interfaces;

public interface IPermisosRepository
{
    Task<bool> TienePermiso(int idUsuario, string entidad, string recurso);
    Task<IEnumerable<PermisoDto>> ObtenerPermisosPorRol(int idRol);
    Task<IEnumerable<EntidadConRecursosDto>> ObtenerEntidadesConRecursos();
    Task AsignarPermisosARol(int idRol, List<PermisoDto> permisos);
}

