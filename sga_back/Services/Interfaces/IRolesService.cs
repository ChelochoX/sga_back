using sga_back.DTOs;
using sga_back.Models;
using sga_back.Request;

namespace sga_back.Services.Interfaces;

public interface IRolesService
{
    Task<int> Insertar(RoleRequest request);
    Task<int> Actualizar(int id, RoleRequest request);
    Task<bool> Eliminar(int id);
    Task<IEnumerable<Rol>> ObtenerTodos();
    Task<Rol?> ObtenerPorId(int id);
    Task<IEnumerable<RolDetalleDto>> ObtenerDetalleRolesPorNombreUsuario(string nombreUsuario);
}
