using sga_back.DTOs;
using sga_back.Models;

namespace sga_back.Repositories.Interfaces;

public interface IRolesRepository
{
    Task<Rol?> ObtenerPorId(int id);
    Task<IEnumerable<Rol>> ObtenerTodos();
    Task<bool> ExisteNombreRol(string nombreRol);
    Task<IEnumerable<RolDetalleDto>> ObtenerDetalleRolesPorNombreUsuario(string nombreUsuario);
    Task<IEnumerable<int>> ObtenerIdsRolesPorUsuario(string nombreUsuario);
    Task AsignarRolAUsuario(string nombreUsuario, int idRol);
    Task RemoverRolDeUsuario(string nombreUsuario, int idRol);

}
