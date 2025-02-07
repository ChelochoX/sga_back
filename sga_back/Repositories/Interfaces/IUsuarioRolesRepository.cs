namespace sga_back.Repositories.Interfaces;

public interface IUsuarioRolesRepository
{
    Task<bool> AsignarRol(int idUsuario, int idRol);
    Task<bool> EliminarRol(int idUsuario, int idRol);
    Task<IEnumerable<int>> ObtenerRolesPorUsuario(int idUsuario);
}
