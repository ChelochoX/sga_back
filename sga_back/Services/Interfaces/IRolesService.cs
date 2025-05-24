using sga_back.DTOs;
using sga_back.Models;

namespace sga_back.Services.Interfaces;

public interface IRolesService
{
    Task<IEnumerable<Rol>> ObtenerTodos();
    Task<Rol?> ObtenerPorId(int id);
    Task<IEnumerable<RolDetalleDto>> ObtenerDetalleRolesPorNombreUsuario(string nombreUsuario);
    Task ActualizarRolesUsuario(string nombreUsuario, IEnumerable<int> nuevosIdsRoles);
}
