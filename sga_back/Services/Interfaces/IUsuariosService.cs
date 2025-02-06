using sga_back.Request;

namespace sga_back.Services.Interfaces;

public interface IUsuariosService
{
    Task<int> Insertar(UsuarioRequest request);
    Task<bool> ActualizarUsuario(UsuarioUpdateRequest request, int idUsuario);
}
