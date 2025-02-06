using sga_back.Models;

namespace sga_back.Repositories.Interfaces;

public interface IRolesRepository
{
    Task<int> Insertar(Role role);
    Task<int> Actualizar(Role role);
    Task<bool> Eliminar(int id);
    Task<Role?> ObtenerPorId(int id);
    Task<IEnumerable<Role>> ObtenerTodos();
    Task<bool> ExisteNombreRol(string nombreRol);
}
