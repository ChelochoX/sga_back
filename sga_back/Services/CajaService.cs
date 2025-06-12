using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class CajaService : ICajaService
{
    private readonly ICajaRepository _cajaRepository;
    private readonly ILogger<CajaService> _logger;

    public CajaService(ICajaRepository cajaRepository, ILogger<CajaService> logger)
    {
        _cajaRepository = cajaRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CajaMovimiento>> ObtenerMovimientos(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _cajaRepository.ObtenerMovimientos(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<CajaAnulacion>> ObtenerAnulaciones()
    {
        return await _cajaRepository.ObtenerAnulaciones();
    }
}
