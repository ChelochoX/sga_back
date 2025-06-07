using sga_back.DTOs;
using sga_back.Models;
using sga_back.Request;

namespace sga_back.Repositories.Interfaces;

public interface IPagosRepository
{
    Task<int> InsertarPagoConDetalles(PagoEncabezado pago, List<PagoDetalle> detalles);
    Task<bool> ActualizarPagoConDetalles(PagoEncabezado pago, List<PagoDetalle> detalles);
    Task<bool> EliminarPago(int idPago);
    Task<PagoEncabezado?> ObtenerPagoPorId(int idPago);
    Task<IEnumerable<PagoDetalle>> ObtenerDetallesPorPago(int idPago);
    Task<IEnumerable<PagoDetalleDto>> ObtenerPagosPendientes(PagoFiltroRequest filtro);
    Task<IEnumerable<PagoDetalleDto>> ObtenerPagosRealizados(PagoFiltroRequest filtro);
}
