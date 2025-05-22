using AutoMapper;
using sga_back.Common;
using sga_back.DTOs;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
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

    public async Task<int> Insertar(RoleRequest request)
    {
        await ValidationHelper.ValidarAsync(request, _serviceProvider);

        Rol rol = _mapper.Map<Rol>(request);
        return await _repository.Insertar(rol);
    }

    public async Task<int> Actualizar(int id, RoleRequest request)
    {
        await ValidationHelper.ValidarAsync(request, _serviceProvider);

        Rol rol = _mapper.Map<Rol>(request);
        rol.IdRol = id;
        return await _repository.Actualizar(rol);
    }

    public async Task<bool> Eliminar(int id) => await _repository.Eliminar(id);

    public async Task<IEnumerable<Rol>> ObtenerTodos() => await _repository.ObtenerTodos();

    public async Task<Rol?> ObtenerPorId(int id) => await _repository.ObtenerPorId(id);

    public async Task<IEnumerable<RolDetalleDto>> ObtenerDetalleRolesPorNombreUsuario(string nombreUsuario)
    {
        return await _repository.ObtenerDetalleRolesPorNombreUsuario(nombreUsuario);
    }

}
