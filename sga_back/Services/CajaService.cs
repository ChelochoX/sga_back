using sga_back.Common;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class CajaService : ICajaService
{
    private readonly ICajaRepository _repository;
    private readonly ILogger<CajaService> _logger;
    private readonly UserContext _userContext;

    public CajaService(ICajaRepository cajaRepository, ILogger<CajaService> logger, UserContext userContext)
    {
        _repository = cajaRepository;
        _logger = logger;
        _userContext = userContext;
    }

    public async Task<IEnumerable<CajaMovimiento>> ObtenerMovimientos(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _repository.ObtenerMovimientos(fechaInicio, fechaFin);
    }

    public async Task AnularMovimientoCaja(int idMovimiento, string motivo)
    {
        string usuario = _userContext.NombreUsuario;
        await _repository.AnularMovimientoCaja(idMovimiento, motivo, usuario);
    }

}
