using Dapper;
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
            DateTime hasta = fechaFin?.Date ?? fechaInicio?.Date ?? DateTime.Today;

            var sql = @"
            SELECT 
                IdMovimiento AS IdMovimiento,
                CAST(Fecha AS DATE) AS Fecha,
                TipoMovimiento AS TipoMovimiento,
                Monto AS Monto,
                Concepto AS Concepto,
                Usuario AS Usuario,
                Referencia AS Referencia,
                FechaCreacion AS FechaCreacion
            FROM CajaMovimientos
            WHERE CAST(Fecha AS DATE) BETWEEN @Desde AND @Hasta
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


    public async Task<IEnumerable<CajaAnulacion>> ObtenerAnulaciones()
    {
        try
        {
            var sql = "SELECT * FROM CajaAnulaciones ORDER BY FechaAnulacion DESC";
            return await _conexion.QueryAsync<CajaAnulacion>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener anulaciones de caja.");
            throw new RepositoryException("No se pudieron obtener las anulaciones.");
        }
    }

    public async Task InsertarMovimiento(CajaMovimiento movimiento)
    {
        try
        {
            _logger.LogInformation("Iniciando inserción de movimiento en caja. Datos: {@Movimiento}", movimiento);

            var sql = @"INSERT INTO CajaMovimientos (Fecha, TipoMovimiento, Monto, Concepto, Usuario, Referencia)
                    VALUES (@Fecha, @TipoMovimiento, @Monto, @Concepto, @Usuario, @Referencia)";

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

    public async Task InsertarAnulacion(CajaAnulacion anulacion)
    {
        try
        {
            var sql = @"INSERT INTO CajaAnulaciones (IdMovimiento, Motivo, UsuarioAnulacion)
                        VALUES (@IdMovimiento, @Motivo, @UsuarioAnulacion)";
            await _conexion.ExecuteAsync(sql, anulacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar anulación.");
            throw new RepositoryException("Error al anular un movimiento.");
        }
    }
}
