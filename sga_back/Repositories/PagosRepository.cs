using Dapper;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using System.Data;

namespace sga_back.Repositories;

public class PagosRepository : IPagosRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<PagosRepository> _logger;

    public PagosRepository(ILogger<PagosRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }


    public async Task<int> InsertarPagoConDetalles(PagoEncabezado pago, List<PagoDetalle> detalles)
    {
        if (_conexion.State != ConnectionState.Open)
        {
            _conexion.Open();
        }

        using var transaction = _conexion.BeginTransaction();
        try
        {
            _logger.LogInformation("Intentando insertar pago con detalles. Total: {Total}, ID Inscripción: {IdInscripcion}",
                pago.Total, pago.IdInscripcion);

            // Verificar si ya existe un pago para la inscripción
            string queryVerificar = "SELECT COUNT(*) FROM Pagos_Encabezado WHERE id_inscripcion = @IdInscripcion";
            int existePago = await _conexion.ExecuteScalarAsync<int>(queryVerificar, new { pago.IdInscripcion }, transaction);

            if (existePago > 0)
            {
                _logger.LogWarning("Ya existe un pago registrado para la inscripción con ID: {IdInscripcion}", pago.IdInscripcion);
                throw new ReglasdeNegocioException("Ya existe un pago para esta inscripción.");
            }

            // Insertar encabezado
            string queryInsertarEncabezado = @"
                INSERT INTO Pagos_Encabezado (id_inscripcion, total, tipo_cuenta, descuento, observacion)
                VALUES (@IdInscripcion, @Total, @TipoCuenta, @Descuento, @Observacion);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            int idPago = await _conexion.ExecuteScalarAsync<int>(queryInsertarEncabezado, pago, transaction);

            // Insertar detalles
            foreach (var detalle in detalles)
            {
                detalle.IdPago = idPago;
                string queryInsertarDetalle = @"
                    INSERT INTO Pagos_Detalle (id_pago, concepto, monto, fecha_vencimiento, tipo_pago, referencia, voucher_numero, estado)
                    VALUES (@IdPago, @Concepto, @Monto, @FechaVencimiento, @TipoPago, @Referencia, @VoucherNumero, @Estado);";

                await _conexion.ExecuteAsync(queryInsertarDetalle, detalle, transaction);
            }

            transaction.Commit();
            _logger.LogInformation("Pago insertado con éxito. ID: {IdPago}", idPago);
            return idPago;
        }
        catch (ReglasdeNegocioException)
        {
            transaction.Rollback();
            throw;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error al insertar pago.");
            throw new RepositoryException("Ocurrió un error al insertar el pago.", ex);
        }
    }

    public async Task<bool> ActualizarPagoConDetalles(PagoEncabezado pago, List<PagoDetalle> detalles)
    {
        using var transaction = _conexion.BeginTransaction();
        try
        {
            _logger.LogInformation("Intentando actualizar pago con ID: {IdPago}", pago.IdPago);

            // Actualizar encabezado
            string queryActualizarEncabezado = @"
                UPDATE Pagos_Encabezado 
                SET total = @Total, tipo_cuenta = @TipoCuenta, descuento = @Descuento, observacion = @Observacion, 
                    factura_numero = @FacturaNumero, recibo_numero = @ReciboNumero
                WHERE id_pago = @IdPago";

            int filasAfectadas = await _conexion.ExecuteAsync(queryActualizarEncabezado, pago, transaction);

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró el pago con ID: {IdPago} para actualizar.", pago.IdPago);
                throw new NoDataFoundException("No se encontró el pago para actualizar.");
            }

            // Eliminar detalles anteriores
            string queryEliminarDetalles = "DELETE FROM Pagos_Detalle WHERE id_pago = @IdPago";
            await _conexion.ExecuteAsync(queryEliminarDetalles, new { pago.IdPago }, transaction);

            // Insertar nuevos detalles
            foreach (var detalle in detalles)
            {
                detalle.IdPago = pago.IdPago;
                string queryInsertarDetalle = @"
                    INSERT INTO Pagos_Detalle (id_pago, concepto, monto, fecha_vencimiento, tipo_pago, referencia, voucher_numero, estado)
                    VALUES (@IdPago, @Concepto, @Monto, @FechaVencimiento, @TipoPago, @Referencia, @VoucherNumero, @Estado);";

                await _conexion.ExecuteAsync(queryInsertarDetalle, detalle, transaction);
            }

            transaction.Commit();
            _logger.LogInformation("Pago actualizado con éxito. ID: {IdPago}", pago.IdPago);
            return true;
        }
        catch (NoDataFoundException)
        {
            transaction.Rollback();
            throw;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error al actualizar pago.");
            throw new RepositoryException("Ocurrió un error al actualizar el pago.", ex);
        }
    }

    public async Task<bool> EliminarPago(int idPago)
    {
        using var transaction = _conexion.BeginTransaction();
        try
        {
            _logger.LogInformation("Intentando eliminar pago con ID: {IdPago}", idPago);

            string queryEliminarDetalles = "DELETE FROM Pagos_Detalle WHERE id_pago = @IdPago";
            string queryEliminarEncabezado = "DELETE FROM Pagos_Encabezado WHERE id_pago = @IdPago";

            await _conexion.ExecuteAsync(queryEliminarDetalles, new { IdPago = idPago }, transaction);
            int filasAfectadas = await _conexion.ExecuteAsync(queryEliminarEncabezado, new { IdPago = idPago }, transaction);

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró el pago con ID: {IdPago} para eliminar.", idPago);
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            _logger.LogInformation("Pago con ID: {IdPago} eliminado exitosamente.", idPago);
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error al eliminar pago.");
            throw new RepositoryException("Ocurrió un error al intentar eliminar el pago.", ex);
        }
    }

    public async Task<PagoEncabezado?> ObtenerPagoPorId(int idPago)
    {
        _logger.LogInformation("Obteniendo pago con ID: {IdPago}", idPago);
        string query = "SELECT * FROM Pagos_Encabezado WHERE id_pago = @IdPago";
        return await _conexion.QueryFirstOrDefaultAsync<PagoEncabezado>(query, new { IdPago = idPago });
    }

    public async Task<IEnumerable<PagoDetalle>> ObtenerDetallesPorPago(int idPago)
    {
        _logger.LogInformation("Obteniendo detalles de pago con ID: {IdPago}", idPago);
        string query = "SELECT * FROM Pagos_Detalle WHERE id_pago = @IdPago";
        return await _conexion.QueryAsync<PagoDetalle>(query, new { IdPago = idPago });
    }

    public async Task<(IEnumerable<PagoCabeceraDto> items, int total)> ObtenerPagosPendientes(PagoFiltroRequest filtro)
    {
        try
        {
            _logger.LogInformation("Obteniendo pagos PENDIENTES agrupados con filtros: {@Filtro}", filtro);

            var sql = @"
        SELECT 
            pe.id_pago AS IdPago,
            pe.id_inscripcion AS IdInscripcion,
            per.nombres + ' ' + per.apellidos AS NombreEstudiante,
            cu.nombre AS NombreCurso,
            COALESCE(pe.total, 0) AS DeudaTotal,
            pe.tipo_cuenta AS TipoCuenta,
            COALESCE(pe.descuento, 0) AS DescuentoCabecera,
            pe.observacion AS Observacion,
            pd.id_detalle AS IdDetallePago,
            pd.concepto AS Concepto,
            COALESCE(pd.monto, 0) AS Monto,
            pd.fecha_vencimiento AS FechaVencimiento,
            pd.fecha_pago AS FechaPago,
            pd.tipo_pago AS TipoPago,
            pd.estado AS Estado

        FROM Pagos_Encabezado pe
        JOIN Inscripciones i ON i.id_inscripcion = pe.id_inscripcion
        JOIN Personas per ON per.id_persona = i.id_persona
        JOIN Cursos cu ON cu.id_curso = i.id_curso -- si quieres el nombre del curso
        LEFT JOIN Pagos_Detalle pd ON pd.id_pago = pe.id_pago
        WHERE
            (@NombreEstudiante IS NULL OR (per.nombres + ' ' + per.apellidos) LIKE CONCAT('%', @NombreEstudiante, '%'))
            AND (@FechaVencimiento IS NULL OR pd.fecha_vencimiento = CONVERT(DATE, @FechaVencimiento, 23))
            AND pd.estado = 'Pendiente'
        ORDER BY
            pe.id_pago, pd.fecha_vencimiento
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
        ";

            // (Total igual que antes)
            var sqlTotal = @"
        SELECT COUNT(DISTINCT pe.id_pago)
        FROM Pagos_Encabezado pe
        JOIN Inscripciones i ON i.id_inscripcion = pe.id_inscripcion
        JOIN Personas per ON per.id_persona = i.id_persona
        JOIN Cursos cu ON cu.id_curso = i.id_curso
        LEFT JOIN Pagos_Detalle pd ON pd.id_pago = pe.id_pago
        WHERE
            (@NombreEstudiante IS NULL OR (per.nombres + ' ' + per.apellidos) LIKE CONCAT('%', @NombreEstudiante, '%'))
            AND (@FechaVencimiento IS NULL OR pd.fecha_vencimiento = CONVERT(DATE, @FechaVencimiento, 23))
            AND pd.estado = 'Pendiente'
        ";

            int offset = (filtro.PageNumber - 1) * filtro.PageSize;

            var parametros = new
            {
                NombreEstudiante = string.IsNullOrWhiteSpace(filtro?.NombreEstudiante) ? null : filtro.NombreEstudiante,
                FechaVencimiento = filtro?.FechaVencimiento,
                Offset = offset,
                PageSize = filtro.PageSize
            };

            // Traemos todos los resultados en plano
            var rows = (await _conexion.QueryAsync<dynamic>(sql, parametros)).ToList();

            // Agrupamos en C#
            var agrupado = rows
                .GroupBy(r => new { r.IdPago, r.IdInscripcion, r.NombreEstudiante, r.NombreCurso, r.DeudaTotal, r.TipoCuenta, r.DescuentoCabecera, r.Observacion })
                .Select(g => new PagoCabeceraDto
                {
                    IdPago = g.Key.IdPago,
                    IdInscripcion = g.Key.IdInscripcion,
                    NombreEstudiante = g.Key.NombreEstudiante,
                    NombreCurso = g.Key.NombreCurso,
                    DeudaTotal = g.Key.DeudaTotal,
                    TipoCuenta = g.Key.TipoCuenta,
                    DescuentoCabecera = g.Key.DescuentoCabecera,
                    Observacion = g.Key.Observacion,
                    Detalles = g
                        .Where(x => x.IdDetallePago != null)
                        .Select(x => new PagoDetalleDto
                        {
                            IdDetallePago = x.IdDetallePago,
                            Concepto = x.Concepto,
                            Monto = x.Monto,
                            FechaVencimiento = x.FechaVencimiento,
                            FechaPago = x.FechaPago,
                            TipoPago = x.TipoPago,
                            Estado = x.Estado,
                        }).ToList()
                }).ToList();

            int total = await _conexion.ExecuteScalarAsync<int>(sqlTotal, parametros);

            return (agrupado, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pagos pendientes agrupados");
            throw new RepositoryException("Error al obtener pagos pendientes agrupados", ex);
        }
    }


    public async Task<(IEnumerable<PagoCabeceraDto> items, int total)> ObtenerPagosRealizados(PagoFiltroRequest filtro)
    {
        try
        {
            _logger.LogInformation("Obteniendo pagos REALIZADOS agrupados con filtros: {@Filtro}", filtro);

            var sql = @"
        SELECT 
            pe.id_pago AS IdPago,
            pe.id_inscripcion AS IdInscripcion,
            per.nombres + ' ' + per.apellidos AS NombreEstudiante,
            cu.nombre AS NombreCurso,
            COALESCE(pe.total, 0) AS DeudaTotal,
            pe.tipo_cuenta AS TipoCuenta,
            COALESCE(pe.descuento, 0) AS DescuentoCabecera,
            pe.observacion AS Observacion,
            pd.id_detalle AS IdDetallePago,
            pd.concepto AS Concepto,
            COALESCE(pd.monto, 0) AS Monto,
            pd.fecha_vencimiento AS FechaVencimiento,
            pd.fecha_pago AS FechaPago,
            pd.tipo_pago AS TipoPago,
            pd.estado AS Estado

        FROM Pagos_Encabezado pe
        JOIN Inscripciones i ON i.id_inscripcion = pe.id_inscripcion
        JOIN Personas per ON per.id_persona = i.id_persona
        JOIN Cursos cu ON cu.id_curso = i.id_curso -- si quieres el nombre del curso
        LEFT JOIN Pagos_Detalle pd ON pd.id_pago = pe.id_pago
        WHERE
            (@NombreEstudiante IS NULL OR (per.nombres + ' ' + per.apellidos) LIKE CONCAT('%', @NombreEstudiante, '%'))
            AND (@FechaVencimiento IS NULL OR pd.fecha_vencimiento = CONVERT(DATE, @FechaVencimiento, 23))
            AND pd.estado = 'Pagado'
        ORDER BY
            pe.id_pago, pd.fecha_vencimiento
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
        ";

            // Para el total, cuenta cabeceras (pagos únicos)
            var sqlTotal = @"
        SELECT COUNT(DISTINCT pe.id_pago)
        FROM Pagos_Encabezado pe
        JOIN Inscripciones i ON i.id_inscripcion = pe.id_inscripcion
        JOIN Personas per ON per.id_persona = i.id_persona
        JOIN Cursos cu ON cu.id_curso = i.id_curso
        LEFT JOIN Pagos_Detalle pd ON pd.id_pago = pe.id_pago
        WHERE
            (@NombreEstudiante IS NULL OR (per.nombres + ' ' + per.apellidos) LIKE CONCAT('%', @NombreEstudiante, '%'))
            AND (@FechaVencimiento IS NULL OR pd.fecha_vencimiento = CONVERT(DATE, @FechaVencimiento, 23))
            AND pd.estado = 'Pagado'
        ";

            int offset = (filtro.PageNumber - 1) * filtro.PageSize;

            var parametros = new
            {
                NombreEstudiante = string.IsNullOrWhiteSpace(filtro?.NombreEstudiante) ? null : filtro.NombreEstudiante,
                FechaVencimiento = filtro?.FechaVencimiento,
                Offset = offset,
                PageSize = filtro.PageSize
            };

            var rows = (await _conexion.QueryAsync<dynamic>(sql, parametros)).ToList();

            var agrupado = rows
                .GroupBy(r => new { r.IdPago, r.IdInscripcion, r.NombreEstudiante, r.NombreCurso, r.DeudaTotal, r.TipoCuenta, r.DescuentoCabecera, r.Observacion })
                .Select(g => new PagoCabeceraDto
                {
                    IdPago = g.Key.IdPago,
                    IdInscripcion = g.Key.IdInscripcion,
                    NombreEstudiante = g.Key.NombreEstudiante,
                    NombreCurso = g.Key.NombreCurso,
                    DeudaTotal = g.Key.DeudaTotal,
                    TipoCuenta = g.Key.TipoCuenta,
                    DescuentoCabecera = g.Key.DescuentoCabecera,
                    Observacion = g.Key.Observacion,
                    Detalles = g
                        .Where(x => x.IdDetallePago != null)
                        .Select(x => new PagoDetalleDto
                        {
                            IdDetallePago = x.IdDetallePago,
                            Concepto = x.Concepto,
                            Monto = x.Monto,
                            FechaVencimiento = x.FechaVencimiento,
                            FechaPago = x.FechaPago,
                            TipoPago = x.TipoPago,
                            Estado = x.Estado,
                            // otros campos...
                        }).ToList()
                }).ToList();

            int total = await _conexion.ExecuteScalarAsync<int>(sqlTotal, parametros);

            return (agrupado, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pagos realizados agrupados");
            throw new RepositoryException("Error al obtener pagos realizados agrupados", ex);
        }
    }




}
