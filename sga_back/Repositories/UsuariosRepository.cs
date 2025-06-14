﻿using Dapper;
using Microsoft.AspNetCore.Identity;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using System.Data;

namespace sga_back.Repositories;

public class UsuariosRepository : IUsuariosRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<UsuariosRepository> _logger;
    private readonly PasswordHasher<string> _passwordHasher;

    public UsuariosRepository(ILogger<UsuariosRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
        _passwordHasher = new PasswordHasher<string>();
    }

    public async Task<int> Insertar(Usuario usuario)
    {
        try
        {
            _logger.LogInformation("Intentando insertar usuario: {NombreUsuario}", usuario.NombreUsuario);

            // Verificar si el nombre de usuario ya existe
            string queryVerificar = "SELECT COUNT(*) FROM Usuarios WHERE nombre_usuario = @NombreUsuario";
            int existe = await _conexion.ExecuteScalarAsync<int>(queryVerificar, new { usuario.NombreUsuario });

            if (existe > 0)
            {
                _logger.LogWarning("El nombre de usuario {NombreUsuario} ya está registrado.", usuario.NombreUsuario);
                throw new ReglasdeNegocioException("El nombre de usuario ya está registrado.");
            }

            usuario.ContrasenaHash = _passwordHasher.HashPassword(null, usuario.ContrasenaHash);

            // Insertar el nuevo usuario
            string queryInsertar = @"
                INSERT INTO Usuarios (id_persona, nombre_usuario, contrasena_hash, estado, fecha_creacion,contrasena_temporal,requiere_cambio_contrasena)
                VALUES (@IdPersona, @NombreUsuario, @ContrasenaHash, @Estado, @FechaCreacion,@ContrasenaTemporal,1);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            usuario.FechaCreacion = DateTime.UtcNow;
            int idUsuario = await _conexion.ExecuteScalarAsync<int>(queryInsertar, usuario);

            // 🔹 Asignar el rol "CambioContrasena" automáticamente
            string queryInsertarRol = @"
            INSERT INTO Usuario_Roles (id_usuario, id_rol)
            VALUES (@IdUsuario, 6)"; // ID del Rol "CambioContrasena"

            await _conexion.ExecuteAsync(queryInsertarRol, new { IdUsuario = idUsuario });

            _logger.LogInformation("Usuario insertado con ID: {IdUsuario}", idUsuario);

            return idUsuario;
        }
        catch (ReglasdeNegocioException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar usuario");
            throw new RepositoryException("Ocurrió un error al intentar insertar el usuario.", ex);
        }
    }

    public async Task<bool> Eliminar(int id)
    {
        try
        {
            _logger.LogInformation("Intentando eliminar usuario con ID: {Id}", id);
            string query = "DELETE FROM Usuarios WHERE id_usuario = @Id";
            int filasAfectadas = await _conexion.ExecuteAsync(query, new { Id = id });

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró el usuario con ID: {Id} para eliminar.", id);
                return false;
            }

            _logger.LogInformation("Usuario con ID: {Id} eliminado exitosamente.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario con ID: {Id}", id);
            throw new RepositoryException("Ocurrió un error al intentar eliminar el usuario.", ex);
        }
    }

    public async Task<bool> ExisteNombreUsuario(string nombreUsuario)
    {
        string query = "SELECT COUNT(*) FROM Usuarios WHERE nombre_usuario = @NombreUsuario";
        int count = await _conexion.QuerySingleAsync<int>(query, new { NombreUsuario = nombreUsuario });
        return count > 0;
    }

    public async Task<bool> ActualizarUsuario(int idUsuario, string nombreUsuario, string nuevaContrasena)
    {
        try
        {
            _logger.LogInformation("Intentando actualizar usuario con ID: {IdUsuario}", idUsuario);

            string hashContrasena = _passwordHasher.HashPassword(null, nuevaContrasena);

            string query = @"
                    UPDATE Usuarios
                    SET nombre_usuario = @NombreUsuario, 
                        contrasena_hash = @ContrasenaHash, 
                        estado = 'Activo', 
                        fecha_modificacion = GETDATE()
                        requiere_cambio_contrasena = 0
                    WHERE id_usuario = @IdUsuario";

            int filasAfectadas = await _conexion.ExecuteAsync(query, new
            {
                IdUsuario = idUsuario,
                NombreUsuario = nombreUsuario,
                ContrasenaHash = nuevaContrasena
            });

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró el usuario con ID: {IdUsuario} para actualizar.", idUsuario);
                return false;
            }

            _logger.LogInformation("Usuario con ID: {IdUsuario} actualizado exitosamente.", idUsuario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario con ID: {IdUsuario}", idUsuario);
            throw new RepositoryException("Ocurrió un error al intentar actualizar el usuario.", ex);
        }
    }

    public async Task<Usuario?> ValidarCredenciales(string usuario, string contrasena)
    {
        try
        {
            _logger.LogInformation("Validando credenciales para el usuario: {Usuario}", usuario);

            string query = @"
            SELECT 
                 u.id_usuario AS IdUsuario,
                 u.id_persona AS IdPersona,
                 ur.id_rol AS IdRol,
                 u.nombre_usuario AS NombreUsuario,
                 u.contrasena_hash AS ContrasenaHash,
                 u.estado AS Estado,
                 u.fecha_creacion AS FechaCreacion,
                 u.fecha_modificacion AS FechaModificacion,
                 u.contrasena_temporal AS ContrasenaTemporal,
                 u.requiere_cambio_contrasena AS RequiereCambioContrasena
             FROM Usuarios u
             INNER JOIN Usuario_Roles ur ON u.id_usuario = ur.id_usuario
             WHERE u.nombre_usuario = @NombreUsuario";

            var usuarioDb = await _conexion.QueryFirstOrDefaultAsync<Usuario>(query, new { NombreUsuario = usuario });

            if (usuarioDb == null)
            {
                _logger.LogWarning("No se encontró usuario con nombre: {Usuario}", usuario);
                return null;
            }

            // Si no se requiere validar contraseña (por ejemplo, al cambiar contraseña)
            if (string.IsNullOrEmpty(contrasena))
            {
                _logger.LogInformation("Contraseña vacía, se retorna usuario solo con datos.");
                return usuarioDb;
            }

            // Validar la contraseña si fue proporcionada
            var resultado = _passwordHasher.VerifyHashedPassword(null, usuarioDb.ContrasenaHash, contrasena);

            if (resultado == PasswordVerificationResult.Success)
            {
                _logger.LogInformation("Validación de contraseña exitosa para el usuario: {Usuario}", usuario);
                return usuarioDb;
            }
            else
            {
                _logger.LogWarning("Contraseña incorrecta para el usuario: {Usuario}", usuario);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar credenciales para el usuario: {Usuario}", usuario);
            throw new RepositoryException("Ocurrió un error al validar las credenciales del usuario.", ex);
        }
    }

    public async Task<bool> ActualizarContrasena(int idUsuario, string nuevaContrasena, string estado, bool requiereCambioContrasena)
    {
        try
        {
            // 🔒 Hashear la nueva contraseña
            string hashContrasena = _passwordHasher.HashPassword(null, nuevaContrasena);

            string query = @"
            UPDATE Usuarios
            SET contrasena_hash = @ContrasenaHash,
                estado = @Estado,
                requiere_cambio_contrasena = @RequiereCambio
            WHERE id_usuario = @IdUsuario";

            int filasAfectadas = await _conexion.ExecuteAsync(query, new
            {
                IdUsuario = idUsuario,
                ContrasenaHash = hashContrasena,
                Estado = estado,
                RequiereCambio = requiereCambioContrasena
            });

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró el usuario con ID: {IdUsuario} para actualizar la contraseña.", idUsuario);
                throw new ReglasdeNegocioException("No se encontró el usuario con ID: {IdUsuario} para actualizar la contraseña.");
            }

            _logger.LogInformation("Contraseña del usuario con ID: {IdUsuario} actualizada exitosamente.", idUsuario);
            return true;

        }
        catch (ReglasdeNegocioException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar usuario");
            throw new RepositoryException("Ocurrió un error al intentar insertar el usuario.", ex);
        }
    }

    public async Task<(IEnumerable<Usuario>, int)> ObtenerUsuarios(string? filtro, int pageNumber, int pageSize)
    {
        try
        {
            _logger.LogInformation("Obteniendo lista de usuarios...");

            // Convertir el filtro a formato adecuado si es una fecha
            if (DateTime.TryParseExact(filtro, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                filtro = parsedDate.ToString("yyyy-MM-dd");
                _logger.LogInformation("Filtro convertido a formato de búsqueda: {Filtro}", filtro);
            }

            // Query para obtener usuarios con paginación y filtrado
            string query = @"
                        SELECT 
                            id_usuario AS IdUsuario,
                            nombre_usuario AS NombreUsuario,
                            estado AS Estado,
                            fecha_creacion AS FechaCreacion,
                            fecha_modificacion AS FechaModificacion,
                            requiere_cambio_contrasena AS RequiereCambioContrasena,
                            contrasena_temporal AS ContrasenaTemporal

                        FROM Usuarios
                        WHERE (@Filtro IS NULL OR 
                            nombre_usuario LIKE '%' + @Filtro + '%' OR 
                            estado LIKE '%' + @Filtro + '%' OR
                            CONVERT(VARCHAR(10), fecha_creacion, 120) LIKE '%' + @Filtro + '%' OR
                            CONVERT(VARCHAR(10), fecha_modificacion, 120) LIKE '%' + @Filtro + '%')
                        ORDER BY nombre_usuario
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                        SELECT COUNT(*) FROM Usuarios
                        WHERE (@Filtro IS NULL OR 
                            nombre_usuario LIKE '%' + @Filtro + '%' OR 
                            estado LIKE '%' + @Filtro + '%' OR
                            CONVERT(VARCHAR(10), fecha_creacion, 120) LIKE '%' + @Filtro + '%' OR
                            CONVERT(VARCHAR(10), fecha_modificacion, 120) LIKE '%' + @Filtro + '%')";

            var offset = (pageNumber - 1) * pageSize;

            // Ejecutamos la consulta utilizando Dapper
            using (var multi = await _conexion.QueryMultipleAsync(query, new
            {
                Filtro = string.IsNullOrEmpty(filtro) ? null : filtro,
                Offset = offset,
                PageSize = pageSize
            }))
            {
                var usuarios = await multi.ReadAsync<Usuario>();
                var total = await multi.ReadSingleAsync<int>();

                _logger.LogInformation("Se obtuvieron {Count} usuarios.", usuarios.AsList().Count);
                return (usuarios, total);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la lista de usuarios");
            throw new RepositoryException("Ocurrió un error al intentar obtener los usuarios.", ex);
        }
    }

    public async Task Actualizar(UsuarioNameUpdateRequest request)
    {
        string query = @"
        UPDATE Usuarios
        SET 
            nombre_usuario = @NombreUsuario,
            estado = @Estado,
            fecha_modificacion = @FechaModificacion
        WHERE id_usuario = @IdUsuario";

        await _conexion.ExecuteAsync(query, new
        {
            request.NombreUsuario,
            request.Estado,
            request.FechaModificacion,
            request.IdUsuario
        });
    }

    public async Task<bool> CambiarEstadoUsuario(int idUsuario)
    {
        try
        {
            _logger.LogInformation("Intentando cambiar el estado del usuario con ID: {IdUsuario}", idUsuario);

            string query = @"
            UPDATE Usuarios
            SET 
                estado = CASE 
                            WHEN estado = 'Activo' THEN 'Inactivo' 
                            ELSE 'Activo' 
                         END,
                fecha_modificacion = GETDATE()
            WHERE id_usuario = @IdUsuario";

            int filasAfectadas = await _conexion.ExecuteAsync(query, new
            {
                IdUsuario = idUsuario
            });

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró el usuario con ID: {IdUsuario} para actualizar.", idUsuario);
                return false;
            }

            _logger.LogInformation("Estado del usuario con ID: {IdUsuario} actualizado exitosamente.", idUsuario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar el estado del usuario con ID: {IdUsuario}", idUsuario);
            throw new RepositoryException("Ocurrió un error al intentar cambiar el estado del usuario.", ex);
        }
    }

    public async Task<Usuario?> ObtenerUsuarioActivoPorId(int idUsuario)
    {
        try
        {
            _logger.LogInformation("Validando existencia y estado del usuario con ID: {IdUsuario}", idUsuario);

            string query = @"
            SELECT 
                id_usuario AS IdUsuario,
                id_persona AS IdPersona,
                nombre_usuario AS NombreUsuario,
                estado AS Estado
            FROM Usuarios
            WHERE id_usuario = @IdUsuario AND estado = 'Activo';";

            var usuario = await _conexion.QueryFirstOrDefaultAsync<Usuario>(query, new { IdUsuario = idUsuario });

            if (usuario == null)
            {
                _logger.LogWarning("Usuario con ID {IdUsuario} no encontrado o no activo.", idUsuario);
            }

            return usuario;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar el usuario por ID");
            throw new RepositoryException("Error al validar el usuario por ID", ex);
        }
    }



}
