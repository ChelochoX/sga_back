using Dapper;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using System.Data;

namespace sga_back.Repositories;

public class PersonasRepository : IPersonasRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<PersonasRepository> _logger;

    public PersonasRepository(ILogger<PersonasRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }

    public async Task<int> Insertar(Persona persona)
    {
        try
        {
            _logger.LogInformation("Intentando insertar persona con Cédula: {Cedula}", persona.Cedula);

            // Verificar si la cédula ya existe
            string queryVerificarCedula = "SELECT COUNT(*) FROM Personas WHERE cedula = @Cedula";
            int existeCedula = await _conexion.ExecuteScalarAsync<int>(queryVerificarCedula, new { persona.Cedula });

            if (existeCedula > 0)
            {
                _logger.LogWarning("No se pudo insertar la persona. La cédula {Cedula} ya está registrada.", persona.Cedula);
                throw new ReglasdeNegocioException("La cédula ya está registrada.");
            }

            // Verificar si el correo ya existe
            string queryVerificarEmail = "SELECT COUNT(*) FROM Personas WHERE email = @Email";
            int existeEmail = await _conexion.ExecuteScalarAsync<int>(queryVerificarEmail, new { persona.Email });

            if (existeEmail > 0)
            {
                _logger.LogWarning("No se pudo insertar la persona. El correo {Email} ya está registrado.", persona.Email);
                throw new ReglasdeNegocioException("El correo electrónico ya está registrado.");
            }

            // Si la cédula no existe, insertar el nuevo registro
            string queryInsertar = @"
                INSERT INTO Personas (nombres, apellidos, email, telefono, direccion, fecha_nacimiento, fecha_registro, cedula, ruc, digito_verificador)
                VALUES (@Nombres, @Apellidos, @Email, @Telefono, @Direccion, @FechaNacimiento, @FechaRegistro, @Cedula, @Ruc, @DigitoVerificador);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            int id = await _conexion.ExecuteScalarAsync<int>(queryInsertar, persona);
            _logger.LogInformation("Persona insertada con ID: {Id}", id);

            return id;
        }
        catch (ReglasdeNegocioException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar persona");
            throw new RepositoryException("Ocurrió un error al intentar insertar la persona.", ex);
        }
    }

    public async Task<int> Actualizar(Persona persona)
    {
        try
        {
            _logger.LogInformation("Intentando actualizar persona con ID: {IdPersona} y Cédula: {Cedula}", persona.IdPersona, persona.Cedula);

            string query = @"
            UPDATE Personas
            SET nombres = @Nombres, apellidos = @Apellidos, email = @Email, telefono = @Telefono,
                direccion = @Direccion, fecha_nacimiento = @FechaNacimiento, cedula = @Cedula, 
                ruc = @Ruc, digito_verificador = @DigitoVerificador
            WHERE id_persona = @IdPersona";

            int filasAfectadas = await _conexion.ExecuteAsync(query, persona);

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró la persona con ID: {IdPersona} para actualizar.", persona.IdPersona);
                throw new NoDataFoundException("No se encontró la persona para actualizar.");
            }

            _logger.LogInformation("Persona con ID: {IdPersona} actualizada exitosamente.", persona.IdPersona);
            return filasAfectadas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar persona con ID: {IdPersona}", persona.IdPersona);
            throw new RepositoryException("Ocurrió un error al intentar actualizar la persona.", ex);
        }
    }

    public async Task<bool> Eliminar(int id)
    {
        try
        {
            _logger.LogInformation("Intentando eliminar persona con ID: {Id}", id);

            string query = "DELETE FROM Personas WHERE id_persona = @Id";
            int filasAfectadas = await _conexion.ExecuteAsync(query, new { Id = id });

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró la persona con ID: {Id} para eliminar.", id);
                return false;
            }

            _logger.LogInformation("Persona con ID: {Id} eliminada exitosamente.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar persona con ID: {Id}", id);
            throw new RepositoryException("Ocurrió un error al intentar eliminar la persona.", ex);
        }
    }

    public async Task<IEnumerable<Persona>> ObtenerPersonas()
    {
        try
        {
            _logger.LogInformation("Obteniendo lista de personas...");

            string query = @"
                SELECT 
                    id_persona AS IdPersona, 
                    nombres AS Nombres, 
                    apellidos AS Apellidos, 
                    email AS Email,
                    telefono AS Telefono,
                    direccion AS Direccion,
                    fecha_nacimiento AS FechaNacimiento,
                    fecha_registro AS FechaRegistro,
                    cedula AS Cedula,
                    ruc AS Ruc,
                    digito_verificador AS DigitoVerificador
                FROM Personas";

            var personas = await _conexion.QueryAsync<Persona>(query);
            _logger.LogInformation("Se obtuvieron {Count} personas.", personas.AsList().Count);

            return personas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la lista de personas");
            throw new RepositoryException("Ocurrió un error al intentar obtener las personas.", ex);
        }
    }
}
