using sga_back.Models;

namespace sga_back.Repositories.Interfaces;

public interface IPagosRepository
{
    Task<int> InsertarPagoConDetalles(PagoEncabezado pago, List<PagoDetalle> detalles);
    Task<bool> ActualizarPagoConDetalles(PagoEncabezado pago, List<PagoDetalle> detalles);
    Task<bool> EliminarPago(int idPago);
    Task<PagoEncabezado?> ObtenerPagoPorId(int idPago);
    Task<IEnumerable<PagoDetalle>> ObtenerDetallesPorPago(int idPago);
}
