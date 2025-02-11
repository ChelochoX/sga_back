using sga_back.Repositories.Interfaces;
using System.Data;

namespace sga_back.Repositories;

public class PagosRepository : IPagosRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<PagosRepository> _logger;

    public PagosRepository(ILogger<PagosRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }


}
