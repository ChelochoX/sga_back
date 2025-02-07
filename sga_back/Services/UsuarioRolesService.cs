using sga_back.Repositories.Interfaces;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class UsuarioRolesService : IUsuarioRolesService

{
    private readonly IUsuarioRolesRepository _repository;
    private readonly ILogger<UsuarioRolesService> _logger;

    public UsuarioRolesService(IUsuarioRolesRepository repository, ILogger<UsuarioRolesService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> AsignarRol(int idUsuario, int idRol)
    {
        _logger.LogInformation("Asignando rol con ID: {IdRol} al usuario con ID: {IdUsuario}", idRol, idUsuario);
        return await _repository.AsignarRol(idUsuario, idRol);
    }

    public async Task<bool> EliminarRol(int idUsuario, int idRol)
    {
        _logger.LogInformation("Eliminando rol con ID: {IdRol} del usuario con ID: {IdUsuario}", idRol, idUsuario);
        return await _repository.EliminarRol(idUsuario, idRol);
    }

    public async Task<IEnumerable<int>> ObtenerRolesPorUsuario(int idUsuario)
    {
        _logger.LogInformation("Obteniendo roles del usuario con ID: {IdUsuario}", idUsuario);
        return await _repository.ObtenerRolesPorUsuario(idUsuario);
    }
}
