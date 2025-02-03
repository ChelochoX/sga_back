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
            string queryVerificar = "SELECT COUNT(*) FROM Personas WHERE cedula = @Cedula";
            int existe = await _conexion.ExecuteScalarAsync<int>(queryVerificar, new { persona.Cedula });

            if (existe > 0)
            {
                _logger.LogWarning("No se pudo insertar la persona. La cédula {Cedula} ya está registrada.", persona.Cedula);
                return 0; // Se devuelve 0 para indicar que la inserción falló por duplicado
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar persona con Cédula: {Cedula}", persona.Cedula);
            throw new RepositoryException("Ocurrió un error al intentar insertar la persona.", ex);
        }
    }


}
