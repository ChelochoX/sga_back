using sga_back.DTOs;

namespace sga_back.Services.Interfaces;

public interface IPermisosService
{
    Task<bool> TienePermiso(int idUsuario, string entidad, string recurso);
    Task<IEnumerable<PermisoDto>> ObtenerPermisosPorRol(int idRol);
    Task<IEnumerable<EntidadConRecursosDto>> ObtenerEntidadesConRecursos();
    Task AsignarPermisosARol(int idRol, List<(int idEntidad, int idRecurso)> permisos);

}
