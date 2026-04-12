using Dapper;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using System.Data;

namespace sga_back.Repositories;

public class InscripcionesRepository : IInscripcionesRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<InscripcionesRepository> _logger;

    public InscripcionesRepository(ILogger<InscripcionesRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }

    public async Task<int> Insertar(Inscripcion inscripcion)
    {
        try
        {
            _logger.LogInformation(
                "Insertando nueva inscripción para persona ID {IdPersona}, curso ID {IdCurso}",
                inscripcion.IdPersona,
                inscripcion.IdCurso);

            const string queryVerificar = @"
                SELECT COUNT(*)
                FROM Inscripciones
                WHERE id_persona = @IdPersona
                  AND id_curso = @IdCurso
                  AND estado = 'Activa';";

            int existe = await _conexion.ExecuteScalarAsync<int>(
                queryVerificar,
                new { inscripcion.IdPersona, inscripcion.IdCurso });

            if (existe > 0)
                throw new ReglasdeNegocioException("La persona ya está inscrita en este curso.");

            const string queryInsertar = @"
                INSERT INTO Inscripciones
                (
                    id_persona,
                    id_curso,
                    fecha_inscripcion,
                    estado,
                    monto_descuento,
                    motivo_descuento,
                    monto_descuento_practica,
                    motivo_descuento_practica,
                    monto_descuento_matricula,
                    motivo_descuento_matricula
                )
                VALUES
                (
                    @IdPersona,
                    @IdCurso,
                    @FechaInscripcion,
                    @Estado,
                    @MontoDescuento,
                    @MotivoDescuento,
                    @MontoDescuentoPractica,
                    @MotivoDescuentoPractica,
                    @MontoDescuentoMatricula,
                    @MotivoDescuentoMatricula
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int id = await _conexion.ExecuteScalarAsync<int>(queryInsertar, inscripcion);

            _logger.LogInformation("Inscripción insertada con ID {IdInscripcion}", id);
            return id;
        }
        catch (ReglasdeNegocioException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar inscripción.");
            throw new RepositoryException("Ocurrió un error al intentar insertar la inscripción.", ex);
        }
    }

    public async Task<int> InsertarConPagos(
        Inscripcion inscripcion,
        PagoEncabezado pagoEncabezado,
        List<PagoDetalle> detalles)
    {
        IDbTransaction? transaction = null;

        try
        {
            if (_conexion.State != ConnectionState.Open)
                _conexion.Open();

            transaction = _conexion.BeginTransaction();

            const string queryVerificar = @"
                SELECT COUNT(*)
                FROM Inscripciones
                WHERE id_persona = @IdPersona
                  AND id_curso = @IdCurso
                  AND estado = 'Activa';";

            int existe = await _conexion.ExecuteScalarAsync<int>(
                queryVerificar,
                new { inscripcion.IdPersona, inscripcion.IdCurso },
                transaction);

            if (existe > 0)
                throw new ReglasdeNegocioException("La persona ya está inscrita en este curso.");

            const string queryInsertarInscripcion = @"
                INSERT INTO Inscripciones
                (
                    id_persona,
                    id_curso,
                    fecha_inscripcion,
                    estado,
                    monto_descuento,
                    motivo_descuento,
                    monto_descuento_practica,
                    motivo_descuento_practica,
                    monto_descuento_matricula,
                    motivo_descuento_matricula
                )
                VALUES
                (
                    @IdPersona,
                    @IdCurso,
                    @FechaInscripcion,
                    @Estado,
                    @MontoDescuento,
                    @MotivoDescuento,
                    @MontoDescuentoPractica,
                    @MotivoDescuentoPractica,
                    @MontoDescuentoMatricula,
                    @MotivoDescuentoMatricula
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int idInscripcion = await _conexion.ExecuteScalarAsync<int>(
                queryInsertarInscripcion,
                inscripcion,
                transaction);

            const string queryInsertarEncabezado = @"
                INSERT INTO Pagos_Encabezado
                (
                    id_inscripcion,
                    total,
                    tipo_cuenta,
                    descuento,
                    observacion
                )
                VALUES
                (
                    @IdInscripcion,
                    @Total,
                    @TipoCuenta,
                    @Descuento,
                    @Observacion
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int idPago = await _conexion.ExecuteScalarAsync<int>(
                queryInsertarEncabezado,
                new
                {
                    IdInscripcion = idInscripcion,
                    pagoEncabezado.Total,
                    pagoEncabezado.TipoCuenta,
                    pagoEncabezado.Descuento,
                    pagoEncabezado.Observacion
                },
                transaction);

            const string queryInsertarDetalle = @"
                INSERT INTO Pagos_Detalle
                (
                    id_pago,
                    concepto,
                    monto,
                    fecha_vencimiento,
                    estado
                )
                VALUES
                (
                    @IdPago,
                    @Concepto,
                    @Monto,
                    @FechaVencimiento,
                    @Estado
                );";

            foreach (var detalle in detalles)
            {
                await _conexion.ExecuteAsync(
                    queryInsertarDetalle,
                    new
                    {
                        IdPago = idPago,
                        detalle.Concepto,
                        detalle.Monto,
                        detalle.FechaVencimiento,
                        detalle.Estado
                    },
                    transaction);
            }

            transaction.Commit();

            _logger.LogInformation(
                "Inscripción y pagos insertados correctamente. IdInscripcion: {IdInscripcion}",
                idInscripcion);

            return idInscripcion;
        }
        catch (ReglasdeNegocioException)
        {
            transaction?.Rollback();
            throw;
        }
        catch (Exception ex)
        {
            transaction?.Rollback();
            _logger.LogError(ex, "Error al insertar inscripción con pagos.");
            throw new RepositoryException(
                "Ocurrió un error al intentar insertar la inscripción con sus pagos.",
                ex);
        }
        finally
        {
            if (_conexion.State == ConnectionState.Open)
                _conexion.Close();
        }
    }

    public async Task<int> Actualizar(Inscripcion inscripcion)
    {
        try
        {
            _logger.LogInformation(
                "Actualizando inscripción con ID {IdInscripcion}",
                inscripcion.IdInscripcion);

            const string query = @"
                UPDATE Inscripciones
                SET estado = @Estado,
                    monto_descuento = @MontoDescuento,
                    motivo_descuento = @MotivoDescuento,
                    monto_descuento_practica = @MontoDescuentoPractica,
                    motivo_descuento_practica = @MotivoDescuentoPractica,
                    monto_descuento_matricula = @MontoDescuentoMatricula,
                    motivo_descuento_matricula = @MotivoDescuentoMatricula
                WHERE id_inscripcion = @IdInscripcion;";

            int filasAfectadas = await _conexion.ExecuteAsync(query, inscripcion);

            if (filasAfectadas == 0)
                throw new NoDataFoundException("No se encontró la inscripción para actualizar.");

            return filasAfectadas;
        }
        catch (NoDataFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar inscripción.");
            throw new RepositoryException("Ocurrió un error al intentar actualizar la inscripción.", ex);
        }
    }

    public async Task<bool> Eliminar(int idInscripcion)
    {
        try
        {
            _logger.LogInformation(
                "Intentando eliminar inscripción con ID {IdInscripcion}",
                idInscripcion);

            const string deleteDetalles = @"
                DELETE pd
                FROM Pagos_Detalle pd
                INNER JOIN Pagos_Encabezado pe ON pd.id_pago = pe.id_pago
                WHERE pe.id_inscripcion = @IdInscripcion;";

            await _conexion.ExecuteAsync(deleteDetalles, new { IdInscripcion = idInscripcion });

            const string deleteEncabezado = @"
                DELETE FROM Pagos_Encabezado
                WHERE id_inscripcion = @IdInscripcion;";

            await _conexion.ExecuteAsync(deleteEncabezado, new { IdInscripcion = idInscripcion });

            const string deleteInscripcion = @"
                DELETE FROM Inscripciones
                WHERE id_inscripcion = @IdInscripcion;";

            int filasAfectadas = await _conexion.ExecuteAsync(deleteInscripcion, new { IdInscripcion = idInscripcion });

            return filasAfectadas > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar inscripción con ID {IdInscripcion}", idInscripcion);
            throw new RepositoryException("Ocurrió un error al intentar eliminar la inscripción.", ex);
        }
    }

    public async Task<Inscripcion?> ObtenerPorId(int idInscripcion)
    {
        try
        {
            const string query = @"
                SELECT
                    id_inscripcion AS IdInscripcion,
                    id_persona AS IdPersona,
                    id_curso AS IdCurso,
                    fecha_inscripcion AS FechaInscripcion,
                    estado AS Estado,
                    monto_descuento AS MontoDescuento,
                    motivo_descuento AS MotivoDescuento,
                    monto_descuento_practica AS MontoDescuentoPractica,
                    motivo_descuento_practica AS MotivoDescuentoPractica,
                    monto_descuento_matricula AS MontoDescuentoMatricula,
                    motivo_descuento_matricula AS MotivoDescuentoMatricula
                FROM Inscripciones
                WHERE id_inscripcion = @IdInscripcion;";

            return await _conexion.QueryFirstOrDefaultAsync<Inscripcion>(
                query,
                new { IdInscripcion = idInscripcion });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener inscripción con ID {IdInscripcion}", idInscripcion);
            throw new RepositoryException("Ocurrió un error al intentar obtener la inscripción.", ex);
        }
    }

    public async Task<IEnumerable<InscripcionDetalleDto>> ObtenerTodas(InscripcionFiltroRequest filtro)
    {
        try
        {
            const string sql = @"
                SELECT
                    i.id_inscripcion AS IdInscripcion,
                    p.id_persona AS IdPersona,
                    p.nombres + ' ' + p.apellidos AS NombreEstudiante,
                    c.id_curso AS IdCurso,
                    c.nombre AS NombreCurso,
                    i.fecha_inscripcion AS FechaInscripcion,
                    i.estado AS Estado,
                    i.monto_descuento AS MontoDescuento,
                    i.motivo_descuento AS MotivoDescuento,
                    i.monto_descuento_practica AS MontoDescPractica,
                    i.motivo_descuento_practica AS MotivoDescPractica,
                    i.monto_descuento_matricula AS MontoDescMatricula,
                    i.motivo_descuento_matricula AS MotivoDescMatricula,
                    ISNULL(pe.total, 0) AS TotalPagos,
                    ISNULL(pd_resumen.CantidadPagos, 0) AS CantidadPagos
                FROM Inscripciones i
                INNER JOIN Personas p ON p.id_persona = i.id_persona
                INNER JOIN Cursos c ON c.id_curso = i.id_curso
                LEFT JOIN Pagos_Encabezado pe ON pe.id_inscripcion = i.id_inscripcion
                LEFT JOIN
                (
                    SELECT
                        pe2.id_inscripcion,
                        COUNT(pd.id_detalle) AS CantidadPagos
                    FROM Pagos_Encabezado pe2
                    LEFT JOIN Pagos_Detalle pd ON pd.id_pago = pe2.id_pago
                    GROUP BY pe2.id_inscripcion
                ) pd_resumen ON pd_resumen.id_inscripcion = i.id_inscripcion
                WHERE
                    (@Alumno IS NULL OR (p.nombres + ' ' + p.apellidos) LIKE CONCAT('%', @Alumno, '%'))
                    AND (@CursoNombre IS NULL OR c.nombre LIKE CONCAT('%', @CursoNombre, '%'))
                    AND (@FechaDesde IS NULL OR i.fecha_inscripcion >= CONVERT(DATE, @FechaDesde, 23))
                    AND (@FechaHasta IS NULL OR i.fecha_inscripcion <= CONVERT(DATE, @FechaHasta, 23))
                ORDER BY i.fecha_inscripcion DESC;";

            var parametros = new
            {
                Alumno = string.IsNullOrWhiteSpace(filtro?.Alumno) ? null : filtro.Alumno,
                CursoNombre = string.IsNullOrWhiteSpace(filtro?.CursoNombre) ? null : filtro.CursoNombre,
                FechaDesde = filtro?.FechaDesde,
                FechaHasta = filtro?.FechaHasta
            };

            return await _conexion.QueryAsync<InscripcionDetalleDto>(sql, parametros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener inscripciones con detalle");
            throw new RepositoryException("Error al obtener inscripciones", ex);
        }
    }

    public async Task<IEnumerable<EstudianteDto>> ObtenerEstudiantes(string? search)
    {
        try
        {
            var filtro = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

            const string sql = @"
                SELECT DISTINCT
                    p.id_persona AS IdPersona,
                    p.nombres AS Nombres,
                    p.apellidos AS Apellidos
                FROM Personas p
                INNER JOIN Usuarios u ON u.id_persona = p.id_persona
                INNER JOIN Usuario_Roles ur ON ur.id_usuario = u.id_usuario
                INNER JOIN Roles r ON r.id_rol = ur.id_rol
                WHERE r.nombre_rol = @Rol
                  AND (
                        @Search IS NULL
                        OR p.nombres LIKE '%' + @Search + '%'
                        OR p.apellidos LIKE '%' + @Search + '%'
                      )
                ORDER BY p.nombres, p.apellidos;";

            return await _conexion.QueryAsync<EstudianteDto>(
                sql,
                new
                {
                    Rol = "Estudiante",
                    Search = filtro
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estudiantes");
            throw new RepositoryException("Error al obtener estudiantes", ex);
        }
    }

    public async Task<IEnumerable<CursosInscripcionDto>> ObtenerCursos(string? search)
    {
        try
        {
            var filtro = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

            const string sql = @"
                SELECT
                    id_curso AS IdCurso,
                    nombre AS Nombre,
                    descripcion AS Descripcion
                FROM dbo.Cursos
                WHERE activo = 1
                  AND (
                        @Search IS NULL
                        OR nombre LIKE '%' + @Search + '%'
                        OR descripcion LIKE '%' + @Search + '%'
                      )
                ORDER BY nombre;";

            return await _conexion.QueryAsync<CursosInscripcionDto>(
                sql,
                new { Search = filtro });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cursos");
            throw new RepositoryException("Error al obtener cursos", ex);
        }
    }
}

