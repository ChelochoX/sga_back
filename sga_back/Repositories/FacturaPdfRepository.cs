using Dapper;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Repositories.Interfaces;
using System.Data;

namespace sga_back.Repositories;

public class FacturaPdfRepository : IFacturaPdfRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<FacturaPdfRepository> _logger;

    public FacturaPdfRepository(IDbConnection conexion, ILogger<FacturaPdfRepository> logger)
    {
        _conexion = conexion;
        _logger = logger;
    }

    public async Task<FacturaPdfDto?> ObtenerFacturaParaPdf(int idFactura)
    {
        try
        {
            const string sqlCabecera = @"
            SELECT TOP 1
                f.id_factura AS IdFactura,
                RIGHT('000' + CAST(f.sucursal AS VARCHAR(3)), 3) + '-' +
                RIGHT('000' + CAST(f.caja AS VARCHAR(3)), 3) + '-' +
                RIGHT('0000000' + CAST(f.numero AS VARCHAR(7)), 7) AS NumeroFactura,
                f.fecha_emision AS FechaEmision,
                ISNULL(f.nombre_cliente, '') AS NombreCliente,
                ISNULL(f.ruc_cliente, '') AS RucCliente,
                ISNULL(p.direccion, '') AS DireccionCliente,
                ISNULL(p.telefono, '') AS TelefonoCliente,
                CASE
                    WHEN UPPER(ISNULL(f.tipo_factura, '')) = 'CREDITO' THEN 'Credito'
                    ELSE 'Contado'
                END AS CondicionVenta,
                ISNULL(f.estado, 'Emitido') AS EstadoFactura,
                ISNULL(f.observacion, '') AS Observacion,
                CAST(ISNULL(f.total_iva10, 0) AS DECIMAL(18,2)) AS TotalIva,
                CAST(ISNULL(f.total_guaranies, 0) AS DECIMAL(18,2)) AS TotalGeneral,
                ca.FechaAnulacion AS FechaAnulacion,
                ISNULL(ca.Motivo, '') AS MotivoAnulacion,
                ISNULL(ca.UsuarioAnulacion, '') AS UsuarioAnulacion
            FROM Facturas f
            LEFT JOIN Personas p ON p.cedula = f.ruc_cliente
            LEFT JOIN CajaMovimientos cm ON cm.IdFactura = f.id_factura
            LEFT JOIN CajaAnulaciones ca ON ca.IdMovimiento = cm.IdMovimiento
            WHERE f.id_factura = @IdFactura
            ORDER BY ca.FechaAnulacion DESC;";

            const string sqlDetalle = @"
            SELECT
                CAST(ISNULL(fd.IdPagoDetalle, fd.id_detalle) AS VARCHAR(20)) AS Codigo,
                ISNULL(fd.descripcion, '') AS Descripcion,
                'Unidad' AS Unidad,
                CAST(ISNULL(fd.cantidad, 0) AS DECIMAL(18,2)) AS Cantidad,
                CAST(ISNULL(fd.precio_unitario, 0) AS DECIMAL(18,2)) AS PrecioUnitario,
                CAST(0 AS DECIMAL(18,2)) AS Descuento,
                CAST(0 AS DECIMAL(18,2)) AS Exentas,
                CAST(0 AS DECIMAL(18,2)) AS Iva5,
                CAST(ISNULL(fd.subtotal, 0) AS DECIMAL(18,2)) AS Iva10,
                CAST(ISNULL(fd.monto_iva, 0) AS DECIMAL(18,2)) AS MontoIva
            FROM Facturas_Detalle fd
            WHERE fd.id_factura = @IdFactura
            ORDER BY fd.id_detalle;";

            var cabecera = await _conexion.QueryFirstOrDefaultAsync<FacturaPdfDto>(
                sqlCabecera,
                new { IdFactura = idFactura });

            if (cabecera is null)
                return null;

            var detalles = await _conexion.QueryAsync<FacturaPdfDetalleDto>(
                sqlDetalle,
                new { IdFactura = idFactura });

            cabecera.Detalles = detalles.ToList();

            return cabecera;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la factura {IdFactura} para PDF.", idFactura);
            throw new RepositoryException("No se pudo obtener la factura para generar el PDF.", ex);
        }
    }


    public async Task<int?> ObtenerIdFacturaPorMovimiento(int idMovimiento)
    {
        try
        {
            const string sql = @"
                SELECT TOP 1 IdFactura
                FROM CajaMovimientos
                WHERE IdMovimiento = @IdMovimiento;";

            return await _conexion.ExecuteScalarAsync<int?>(
                sql,
                new { IdMovimiento = idMovimiento });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener IdFactura del movimiento {IdMovimiento}.", idMovimiento);
            throw new RepositoryException("No se pudo obtener la factura asociada al movimiento.", ex);
        }
    }

    public async Task<int?> ObtenerIdFacturaPorAnulacion(int idAnulacion)
    {
        try
        {
            const string sql = @"
                SELECT TOP 1 cm.IdFactura
                FROM CajaAnulaciones ca
                INNER JOIN CajaMovimientos cm ON cm.IdMovimiento = ca.IdMovimiento
                WHERE ca.IdAnulacion = @IdAnulacion;";

            return await _conexion.ExecuteScalarAsync<int?>(
                sql,
                new { IdAnulacion = idAnulacion });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener IdFactura desde la anulación {IdAnulacion}.", idAnulacion);
            throw new RepositoryException("No se pudo obtener la factura asociada a la anulación.", ex);
        }
    }
}
