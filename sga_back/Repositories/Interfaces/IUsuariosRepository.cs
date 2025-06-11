using sga_back.Models;
using sga_back.Request;

namespace sga_back.Repositories.Interfaces;

public interface IUsuariosRepository
{
    Task<int> Insertar(Usuario usuario);
    Task<bool> Eliminar(int id);
    Task<bool> ExisteNombreUsuario(string nombreUsuario);
    Task<bool> ActualizarUsuario(int idUsuario, string nombreUsuario, string nuevaContrasena);
    Task<Usuario?> ValidarCredenciales(string usuario, string contrasena);
    Task<bool> ActualizarContrasena(int idUsuario, string nuevaContrasena, string estado, bool requiereCambioContrasena);
    Task<(IEnumerable<Usuario>, int)> ObtenerUsuarios(string? filtro, int pageNumber, int pageSize);
    Task Actualizar(UsuarioNameUpdateRequest request);
    Task<bool> CambiarEstadoUsuario(int idUsuario);
    Task<Usuario?> ObtenerUsuarioActivoPorId(int idUsuario);
}
