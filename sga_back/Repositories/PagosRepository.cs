using Dapper;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
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

}
