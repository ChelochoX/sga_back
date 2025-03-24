using AutoMapper;
using sga_back.Common;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class UsuariosService : IUsuariosService
{
    private readonly IUsuariosRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsuariosService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public UsuariosService(ILogger<UsuariosService> logger, IUsuariosRepository repository, IMapper mapper, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _serviceProvider = serviceProvider;
    }

    public async Task<int> Insertar(UsuarioRequest request)
    {
        await ValidationHelper.ValidarAsync(request, _serviceProvider);

        _logger.LogInformation("Insertando usuario con nombre: {NombreUsuario}", request.NombreUsuario);

        Usuario usuario = _mapper.Map<Usuario>(request);
        usuario.ContrasenaHash = request.Contrasena;

        int id = await _repository.Insertar(usuario);
        return id;
    }

    public async Task<bool> ActualizarUsuario(UsuarioUpdateRequest request, int idUsuario)
    {
        await ValidationHelper.ValidarAsync(request, _serviceProvider);

        bool existeNombre = await _repository.ExisteNombreUsuario(request.NombreUsuario);
        if (existeNombre)
        {
            throw new ReglasdeNegocioException("El nombre de usuario ya está registrado.");
        }

        return await _repository.ActualizarUsuario(idUsuario, request.NombreUsuario, request.NuevaContrasena);
    }

    public async Task<Usuario?> ValidarCredenciales(string usuario, string contrasena)
    {
        return await _repository.ValidarCredenciales(usuario, contrasena);
    }

    public async Task<bool> CambiarContrasena(int idUsuario, string nuevaContrasena)
    {
        return await _repository.ActualizarContrasena(
            idUsuario,
            nuevaContrasena,
            estado: "Activo",
            requiereCambioContrasena: false
        );
    }
}
