using sga_back.Models;
using sga_back.Request;

namespace sga_back.Services.Interfaces;

public interface IPagosService
{
    Task<int> InsertarPago(PagoEncabezadoRequest request);
    Task<bool> ActualizarPago(PagoEncabezadoRequest request, int idPago);
    Task<PagoEncabezado?> ObtenerPagoPorId(int idPago);
}
