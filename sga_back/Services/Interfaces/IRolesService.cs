using sga_back.Models;
using sga_back.Request;

namespace sga_back.Services.Interfaces;

public interface IRolesService
{
    Task<int> Insertar(RoleRequest request);
    Task<int> Actualizar(int id, RoleRequest request);
    Task<bool> Eliminar(int id);
    Task<IEnumerable<Role>> ObtenerTodos();
    Task<Role?> ObtenerPorId(int id);
}
