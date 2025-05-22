using sga_back.DTOs;
using sga_back.Models;

namespace sga_back.Repositories.Interfaces;

public interface IRolesRepository
{
    Task<int> Insertar(Rol role);
    Task<int> Actualizar(Rol role);
    Task<bool> Eliminar(int id);
    Task<Rol?> ObtenerPorId(int id);
    Task<IEnumerable<Rol>> ObtenerTodos();
    Task<bool> ExisteNombreRol(string nombreRol);
    Task<IEnumerable<RolDetalleDto>> ObtenerDetalleRolesPorNombreUsuario(string nombreUsuario);
}
