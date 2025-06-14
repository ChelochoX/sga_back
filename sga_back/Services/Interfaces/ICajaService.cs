using sga_back.Models;

namespace sga_back.Services.Interfaces;

public interface ICajaService
{
    Task<IEnumerable<CajaMovimiento>> ObtenerMovimientos(DateTime? fechaInicio, DateTime? fechaFin);
    //Task<IEnumerable<CajaAnulacion>> ObtenerAnulaciones();
    Task AnularMovimientoCaja(int idMovimiento, string motivo);
}
