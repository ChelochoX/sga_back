namespace sga_back.Services.Interfaces;

public interface IUsuarioRolesService
{
    Task<bool> AsignarRol(int idUsuario, int idRol);
    Task<bool> EliminarRol(int idUsuario, int idRol);
    Task<IEnumerable<int>> ObtenerRolesPorUsuario(int idUsuario);
}
