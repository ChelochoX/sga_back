using sga_back.DTOs;
using sga_back.Request;
using sga_back.Response;

namespace sga_back.Services.Interfaces;

public interface IPagosService
{
    Task<int> InsertarPagoConDetalles(PagoRequest request);
    Task<bool> ActualizarPagoConDetalles(int idPago, PagoRequest request);
    Task<bool> EliminarPago(int idPago);
    Task<PagoResponse?> ObtenerPagoPorId(int idPago);
    Task<(IEnumerable<PagoCabeceraDto> items, int total)> ObtenerPagosPendientes(PagoFiltroRequest filtro);
    Task<(IEnumerable<PagoCabeceraDto> items, int total)> ObtenerPagosRealizados(PagoFiltroRequest filtro);
    Task RegistrarFacturaContado(FacturaContadoRequest request);

}
