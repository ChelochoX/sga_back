using sga_back.DTOs;

namespace sga_back.Repositories.Interfaces;

public interface IFacturaPdfRepository
{
    Task<FacturaPdfDto?> ObtenerFacturaParaPdf(int idFactura);
    Task<int?> ObtenerIdFacturaPorMovimiento(int idMovimiento);
    Task<int?> ObtenerIdFacturaPorAnulacion(int idAnulacion);
}
