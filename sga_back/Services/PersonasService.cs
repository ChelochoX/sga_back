using AutoMapper;
using FluentValidation;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using sga_back.Services.Interfaces;
using ValidationException = FluentValidation.ValidationException;

namespace sga_back.Services;

public class PersonasService : IPersonasService
{
    private readonly IPersonasRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<PersonasService> _logger;
    private readonly IValidator<PersonaRequest> _validator;
    private readonly IUsuariosRepository _usuariosRepository;

    public PersonasService(ILogger<PersonasService> logger, IPersonasRepository repository, IMapper mapper, IValidator<PersonaRequest> validator, IUsuariosRepository usuariosRepository)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
        _usuariosRepository = usuariosRepository;
    }

    public async Task<int> Insertar(PersonaRequest request)
    {

        FluentValidation.Results.ValidationResult validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        _logger.LogInformation("Insertando persona con cédula: {Cedula}", request.Cedula);

        // Mapeo de request a modelo
        Persona persona = _mapper.Map<Persona>(request);
        persona.FechaRegistro = DateTime.UtcNow;

        // Llamar al repositorio para insertar la persona
        int id = await _repository.Insertar(persona);

        // Si el repositorio retorna 0, significa que la cédula ya está registrada
        if (id == 0)
        {
            _logger.LogWarning("No se pudo insertar la persona. La cédula {Cedula} ya existe.", request.Cedula);
            throw new InvalidOperationException("La cédula ya está registrada.");
        }

        _logger.LogInformation("Persona insertada exitosamente con ID: {Id}", id);

        // Crear automáticamente el usuario inactivo
        await CrearUsuarioAutomaticoParaPersona(id, persona.Nombres, persona.Apellidos);

        return id;
    }

    public async Task<int> Actualizar(int id, PersonaRequest request)
    {
        Persona persona = _mapper.Map<Persona>(request);
        persona.IdPersona = id;

        int filasAfectadas = await _repository.Actualizar(persona);
        return filasAfectadas == 0 ? throw new InvalidOperationException("No se encontró la persona para actualizar.") : filasAfectadas;
    }

    public async Task<bool> Eliminar(int id)
    {
        bool eliminado = await _repository.Eliminar(id);
        return !eliminado ? throw new InvalidOperationException("No se encontró la persona para eliminar.") : eliminado;
    }

    private async Task CrearUsuarioAutomaticoParaPersona(int idPersona, string nombres, string apellidos)
    {
        // Generar nombre de usuario único
        string nombreUsuario = await GenerarNombreUsuarioUnico(nombres, apellidos);

        // Crear objeto usuario
        Usuario nuevoUsuario = new()
        {
            IdPersona = idPersona,
            NombreUsuario = nombreUsuario,
            ContrasenaHash = string.Empty,  // Sin contraseña por ahora
            Estado = "Inactivo",
            FechaCreacion = DateTime.UtcNow
        };

        // Insertar el usuario en la base de datos
        await _usuariosRepository.Insertar(nuevoUsuario);

        _logger.LogInformation("Usuario creado automáticamente para la persona con ID: {IdPersona}", idPersona);
    }

    private async Task<string> GenerarNombreUsuarioUnico(string nombres, string apellidos)
    {
        // Tomar los primeros 3 caracteres de los nombres y apellidos
        string nombreBase = $"{nombres[..Math.Min(5, nombres.Length)].ToLower()}.{apellidos[..Math.Min(5, apellidos.Length)].ToLower()}";

        // Generar un hash corto basado en GUID para evitar duplicados
        string hashUnico = Guid.NewGuid().ToString("N").Substring(0, 4);  // 4 caracteres únicos

        // Formar el nombre de usuario completo
        string nombreUsuario = $"{nombreBase}{hashUnico}";

        // Verificar si ya existe en la base de datos
        while (await _usuariosRepository.ExisteNombreUsuario(nombreUsuario))
        {
            hashUnico = Guid.NewGuid().ToString("N").Substring(0, 4);  // Generar un nuevo hash si hay conflicto
            nombreUsuario = $"{nombreBase}{hashUnico}";
        }

        return nombreUsuario;
    }
}
