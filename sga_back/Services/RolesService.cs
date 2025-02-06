using AutoMapper;
using sga_back.Common;
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

        Role role = _mapper.Map<Role>(request);
        return await _repository.Insertar(role);
    }

    public async Task<int> Actualizar(int id, RoleRequest request)
    {
        await ValidationHelper.ValidarAsync(request, _serviceProvider);

        Role role = _mapper.Map<Role>(request);
        role.IdRol = id;
        return await _repository.Actualizar(role);
    }

    public async Task<bool> Eliminar(int id) => await _repository.Eliminar(id);

    public async Task<IEnumerable<Role>> ObtenerTodos() => await _repository.ObtenerTodos();

    public async Task<Role?> ObtenerPorId(int id) => await _repository.ObtenerPorId(id);

}
