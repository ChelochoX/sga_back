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
            persona.Nombres = persona.Nombres?.Trim().ToUpperInvariant() ?? "";
            persona.Apellidos = persona.Apellidos?.Trim().ToUpperInvariant() ?? "";
            persona.Email = persona.Email?.Trim().ToUpperInvariant() ?? "";
            persona.Telefono = persona.Telefono?.Trim().ToUpperInvariant() ?? "";
            persona.Direccion = persona.Direccion?.Trim().ToUpperInvariant() ?? "";
            persona.Cedula = persona.Cedula?.Trim().ToUpperInvariant() ?? "";
            persona.Ruc = persona.Ruc?.Trim().ToUpperInvariant() ?? "";

            _logger.LogInformation("Intentando insertar persona con Cédula: {Cedula}", persona.Cedula);

            string queryVerificarCedula = @"
            SELECT COUNT(*)
            FROM Personas
            WHERE UPPER(cedula) = @Cedula;";

            int existeCedula = await _conexion.ExecuteScalarAsync<int>(
                queryVerificarCedula,
                new { persona.Cedula });

            if (existeCedula > 0)
            {
                _logger.LogWarning("No se pudo insertar la persona. La cédula {Cedula} ya está registrada.", persona.Cedula);
                throw new ReglasdeNegocioException("La cédula ya está registrada.");
            }

            string queryVerificarEmail = @"
            SELECT COUNT(*)
            FROM Personas
            WHERE UPPER(email) = @Email;";

            int existeEmail = await _conexion.ExecuteScalarAsync<int>(
                queryVerificarEmail,
                new { persona.Email });

            if (existeEmail > 0)
            {
                _logger.LogWarning("No se pudo insertar la persona. El correo {Email} ya está registrado.", persona.Email);
                throw new ReglasdeNegocioException("El correo electrónico ya está registrado.");
            }

            string queryInsertar = @"
            INSERT INTO Personas
            (
                nombres,
                apellidos,
                email,
                telefono,
                direccion,
                fecha_nacimiento,
                fecha_registro,
                cedula,
                ruc,
                digito_verificador
            )
            VALUES
            (
                @Nombres,
                @Apellidos,
                @Email,
                @Telefono,
                @Direccion,
                @FechaNacimiento,
                @FechaRegistro,
                @Cedula,
                @Ruc,
                @DigitoVerificador
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);";

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
            persona.Nombres = persona.Nombres?.Trim().ToUpperInvariant() ?? string.Empty;
            persona.Apellidos = persona.Apellidos?.Trim().ToUpperInvariant() ?? string.Empty;
            persona.Email = persona.Email?.Trim().ToUpperInvariant() ?? string.Empty;
            persona.Telefono = persona.Telefono?.Trim().ToUpperInvariant() ?? string.Empty;
            persona.Direccion = persona.Direccion?.Trim().ToUpperInvariant() ?? string.Empty;
            persona.Cedula = persona.Cedula?.Trim().ToUpperInvariant() ?? string.Empty;
            persona.Ruc = persona.Ruc?.Trim().ToUpperInvariant() ?? string.Empty;

            _logger.LogInformation(
                "Intentando actualizar persona con ID: {IdPersona} y Cédula: {Cedula}",
                persona.IdPersona,
                persona.Cedula);

            string queryVerificarCedula = @"
            SELECT COUNT(*)
            FROM Personas
            WHERE UPPER(cedula) = @Cedula
              AND id_persona <> @IdPersona;";

            int existeCedula = await _conexion.ExecuteScalarAsync<int>(
                queryVerificarCedula,
                new
                {
                    persona.Cedula,
                    persona.IdPersona
                });

            if (existeCedula > 0)
            {
                _logger.LogWarning(
                    "No se pudo actualizar la persona con ID: {IdPersona}. La cédula {Cedula} ya está registrada en otro registro.",
                    persona.IdPersona,
                    persona.Cedula);

                throw new ReglasdeNegocioException("La cédula ya está registrada.");
            }

            string queryVerificarEmail = @"
            SELECT COUNT(*)
            FROM Personas
            WHERE UPPER(email) = @Email
              AND id_persona <> @IdPersona;";

            int existeEmail = await _conexion.ExecuteScalarAsync<int>(
                queryVerificarEmail,
                new
                {
                    persona.Email,
                    persona.IdPersona
                });

            if (existeEmail > 0)
            {
                _logger.LogWarning(
                    "No se pudo actualizar la persona con ID: {IdPersona}. El correo {Email} ya está registrado en otro registro.",
                    persona.IdPersona,
                    persona.Email);

                throw new ReglasdeNegocioException("El correo electrónico ya está registrado.");
            }

            string queryActualizar = @"
            UPDATE Personas
            SET nombres = @Nombres,
                apellidos = @Apellidos,
                email = @Email,
                telefono = @Telefono,
                direccion = @Direccion,
                fecha_nacimiento = @FechaNacimiento,
                cedula = @Cedula,
                ruc = @Ruc,
                digito_verificador = @DigitoVerificador
            WHERE id_persona = @IdPersona;";

            int filasAfectadas = await _conexion.ExecuteAsync(queryActualizar, persona);

            if (filasAfectadas == 0)
            {
                _logger.LogWarning(
                    "No se encontró la persona con ID: {IdPersona} para actualizar.",
                    persona.IdPersona);

                throw new NoDataFoundException("No se encontró la persona para actualizar.");
            }

            _logger.LogInformation(
                "Persona con ID: {IdPersona} actualizada exitosamente.",
                persona.IdPersona);

            return filasAfectadas;
        }
        catch (ReglasdeNegocioException)
        {
            throw;
        }
        catch (NoDataFoundException)
        {
            throw;
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
            _logger.LogInformation("Intentando eliminar persona con ID: {IdPersona}", id);

            const string query = @"
            DELETE FROM Personas
            WHERE id_persona = @Id;";

            int filasAfectadas = await _conexion.ExecuteAsync(query, new { Id = id });

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró la persona con ID: {IdPersona} para eliminar.", id);
                throw new NoDataFoundException("No se encontró la persona para eliminar.");
            }

            _logger.LogInformation("Persona con ID: {IdPersona} eliminada exitosamente.", id);
            return true;
        }
        catch (NoDataFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar persona con ID: {IdPersona}", id);
            throw new RepositoryException("Ocurrió un error al intentar eliminar la persona.", ex);
        }
    }

    public async Task<(IEnumerable<Persona>, int)> ObtenerPersonas(string? filtro, int pageNumber, int pageSize)
    {
        try
        {
            _logger.LogInformation(
                "Obteniendo lista de personas. Filtro: {Filtro}, Página: {PageNumber}, TamañoPágina: {PageSize}",
                filtro, pageNumber, pageSize);

            if (pageNumber <= 0)
                pageNumber = 1;

            if (pageSize <= 0)
                pageSize = 10;

            filtro = string.IsNullOrWhiteSpace(filtro)
                ? null
                : filtro.Trim().ToUpperInvariant();

            if (!string.IsNullOrWhiteSpace(filtro) &&
                DateTime.TryParseExact(
                    filtro,
                    "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime parsedDate))
            {
                filtro = parsedDate.ToString("yyyy-MM-dd");
                _logger.LogInformation("Filtro convertido a formato de búsqueda de fecha: {Filtro}", filtro);
            }

            const string query = @"
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
            FROM Personas
            WHERE (
                @Filtro IS NULL OR
                UPPER(nombres) LIKE '%' + @Filtro + '%' OR
                UPPER(apellidos) LIKE '%' + @Filtro + '%' OR
                UPPER(email) LIKE '%' + @Filtro + '%' OR
                UPPER(telefono) LIKE '%' + @Filtro + '%' OR
                UPPER(direccion) LIKE '%' + @Filtro + '%' OR
                UPPER(cedula) LIKE '%' + @Filtro + '%' OR
                UPPER(ruc) LIKE '%' + @Filtro + '%' OR
                CONVERT(VARCHAR(10), fecha_nacimiento, 120) LIKE '%' + @Filtro + '%' OR
                CONVERT(VARCHAR(10), fecha_registro, 120) LIKE '%' + @Filtro + '%'
            )
            ORDER BY nombres
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Personas
            WHERE (
                @Filtro IS NULL OR
                UPPER(nombres) LIKE '%' + @Filtro + '%' OR
                UPPER(apellidos) LIKE '%' + @Filtro + '%' OR
                UPPER(email) LIKE '%' + @Filtro + '%' OR
                UPPER(telefono) LIKE '%' + @Filtro + '%' OR
                UPPER(direccion) LIKE '%' + @Filtro + '%' OR
                UPPER(cedula) LIKE '%' + @Filtro + '%' OR
                UPPER(ruc) LIKE '%' + @Filtro + '%' OR
                CONVERT(VARCHAR(10), fecha_nacimiento, 120) LIKE '%' + @Filtro + '%' OR
                CONVERT(VARCHAR(10), fecha_registro, 120) LIKE '%' + @Filtro + '%'
            );";

            int offset = (pageNumber - 1) * pageSize;

            using var multi = await _conexion.QueryMultipleAsync(query, new
            {
                Filtro = filtro,
                Offset = offset,
                PageSize = pageSize
            });

            var personas = (await multi.ReadAsync<Persona>()).ToList();
            int total = await multi.ReadSingleAsync<int>();

            _logger.LogInformation("Se obtuvieron {Count} personas. Total de registros: {Total}", personas.Count, total);

            return (personas, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la lista de personas");
            throw new RepositoryException("Ocurrió un error al intentar obtener las personas.", ex);
        }
    }

}
