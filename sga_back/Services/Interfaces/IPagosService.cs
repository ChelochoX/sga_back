using sga_back.Request;
using sga_back.Response;

namespace sga_back.Services.Interfaces;

public interface IPagosService
{
    Task<int> InsertarPagoConDetalles(PagoRequest request);
    Task<bool> ActualizarPagoConDetalles(int idPago, PagoRequest request);
    Task<bool> EliminarPago(int idPago);
    Task<PagoResponse?> ObtenerPagoPorId(int idPago);
}
