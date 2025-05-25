using AutoMapper;
using sga_back.DTOs;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class RolesService : IRolesService
{
    private readonly IRolesRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<RolesService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RolesService(ILogger<RolesService> logger, IRolesRepository repository, IMapper mapper, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _serviceProvider = serviceProvider;
    }

    public async Task<IEnumerable<Rol>> ObtenerTodos() => await _repository.ObtenerTodos();

    public async Task<Rol?> ObtenerPorId(int id) => await _repository.ObtenerPorId(id);

    public async Task<IEnumerable<RolDetalleDto>> ObtenerDetalleRolesPorNombreUsuario(string nombreUsuario)
    {
        return await _repository.ObtenerDetalleRolesPorNombreUsuario(nombreUsuario);
    }

    public async Task ActualizarRolesUsuario(string nombreUsuario, IEnumerable<int> nuevosIdsRoles)
    {
        try
        {
            var rolesActuales = (await _repository.ObtenerIdsRolesPorUsuario(nombreUsuario)).ToList();
            var rolesAAsignar = nuevosIdsRoles.Except(rolesActuales).ToList();
            var rolesARemover = rolesActuales.Except(nuevosIdsRoles).ToList();

            foreach (var idRol in rolesAAsignar.Where(r => r > 0))
            {
                await _repository.AsignarRolAUsuario(nombreUsuario, idRol);
            }

            foreach (var idRol in rolesARemover)
            {
                await _repository.RemoverRolDeUsuario(nombreUsuario, idRol);
            }

            _logger.LogInformation("Roles actualizados para el usuario {NombreUsuario}", nombreUsuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar roles para el usuario {NombreUsuario}", nombreUsuario);
            throw;
        }
    }

}
