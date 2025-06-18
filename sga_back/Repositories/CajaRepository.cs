using Dapper;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using System.Data;

namespace sga_back.Repositories;

public class CajaRepository : ICajaRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<CajaRepository> _logger;

    public CajaRepository(ILogger<CajaRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }

    public async Task<IEnumerable<CajaMovimiento>> ObtenerMovimientos(DateTime? fechaInicio, DateTime? fechaFin)
    {
        try
        {
            _logger.LogInformation("Obteniendo movimientos de caja con parámetros: FechaInicio={FechaInicio}, FechaFin={FechaFin}", fechaInicio, fechaFin);

            DateTime desde = fechaInicio?.Date ?? DateTime.Today;
            DateTime hasta = (fechaFin?.Date.AddDays(1).AddSeconds(-1)) ?? DateTime.Today.AddDays(1).AddSeconds(-1);

            var sql = @"
            SELECT 
                IdMovimiento AS IdMovimiento,
                CAST(Fecha AS DATE) AS Fecha,
                TipoMovimiento AS TipoMovimiento,
                Monto AS Monto,
                Concepto AS Concepto,
                Usuario AS Usuario,
                Referencia AS Referencia,
                FechaCreacion AS FechaCreacion,
                IdFactura AS IdFactura

            FROM CajaMovimientos
            WHERE CAST(Fecha AS DATE) BETWEEN @Desde AND @Hasta
            AND Estado = 'Activo'
            ORDER BY Fecha DESC";

            var movimientos = await _conexion.QueryAsync<CajaMovimiento>(sql, new { Desde = desde, Hasta = hasta });

            _logger.LogInformation("Se recuperaron {Cantidad} movimientos de caja.", movimientos.Count());

            return movimientos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos de caja de forma flexible.");
            throw new RepositoryException("No se pudieron obtener los movimientos.");
        }
    }
    public async Task InsertarMovimiento(CajaMovimiento movimiento)
    {
        try
        {
            _logger.LogInformation("Iniciando inserción de movimiento en caja. Datos: {@Movimiento}", movimiento);

            var sql = @"
                INSERT INTO CajaMovimientos (
                    Fecha, TipoMovimiento, Monto, Concepto,
                    Usuario, Referencia, FechaCreacion, IdFactura
                )
                VALUES (
                    @Fecha, @TipoMovimiento, @Monto, @Concepto,
                    @Usuario, @Referencia, GETDATE(), @IdFactura
                );";

            await _conexion.ExecuteScalarAsync(sql, movimiento);

            _logger.LogInformation("Movimiento en caja insertado correctamente. Fecha: {Fecha}, Monto: {Monto}, Usuario: {Usuario}",
                movimiento.Fecha, movimiento.Monto, movimiento.Usuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar movimiento en caja. Datos: {@Movimiento}", movimiento);
            throw new RepositoryException("Error al insertar un movimiento.");
        }
    }

    public async Task AnularMovimientoCaja(int idMovimiento, string motivo, string usuario)
    {
        try
        {
            await _conexion.ExecuteAsync(
                "sp_AnularMovimientoCaja",
                new { IdMovimiento = idMovimiento, Motivo = motivo, UsuarioAnulacion = usuario },
                commandType: CommandType.StoredProcedure
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular el movimiento vía SP.");
            throw new RepositoryException("Error al anular el movimiento.", ex);
        }
    }

    public async Task<IEnumerable<CajaAnulacionDto>> ObtenerAnulaciones(DateTime? desde, DateTime? hasta)
    {
        try
        {
            _logger.LogInformation("Obteniendo anulaciones de caja. Filtros: Desde = {Desde}, Hasta = {Hasta}", desde, hasta);

            var sql = @"
            SELECT 
                IdAnulacion,
                IdMovimiento,
                Motivo,
                UsuarioAnulacion,
                FechaAnulacion
            FROM CajaAnulaciones
            WHERE 
                (@Desde IS NULL OR FechaAnulacion >= @Desde)
                AND (@Hasta IS NULL OR FechaAnulacion < DATEADD(DAY, 1, @Hasta))
            ORDER BY FechaAnulacion DESC;";

            var result = await _conexion.QueryAsync<CajaAnulacionDto>(sql, new { Desde = desde, Hasta = hasta });

            _logger.LogInformation("Se obtuvieron {Cantidad} anulaciones de caja.", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener anulaciones de caja.");
            throw new RepositoryException("Error al obtener las anulaciones de caja.");
        }
    }



}
