using sga_back.Models;

namespace sga_back.Repositories.Interfaces;

public interface ICajaRepository
{
    Task<IEnumerable<CajaMovimiento>> ObtenerMovimientos(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<CajaAnulacion>> ObtenerAnulaciones();
    Task InsertarMovimiento(CajaMovimiento movimiento);
    Task InsertarAnulacion(CajaAnulacion anulacion);
}
