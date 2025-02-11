using sga_back.Models;

namespace sga_back.Repositories.Interfaces;

public interface IPagosRepository
{
    Task<int> InsertarEncabezado(PagoEncabezado pago);
    Task<int> InsertarDetalle(PagoDetalle detalle);
    Task<int> ActualizarEncabezado(PagoEncabezado pago);
    Task<int> ActualizarDetalle(PagoDetalle detalle);
    Task<bool> EliminarEncabezado(int idPago);
    Task<bool> EliminarDetalle(int idDetalle);
    Task<PagoEncabezado?> ObtenerEncabezadoPorId(int idPago);
    Task<IEnumerable<PagoDetalle>> ObtenerDetallesPorPago(int idPago);
}
