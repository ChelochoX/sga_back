using sga_back.Models;

namespace sga_back.Repositories.Interfaces;

public interface IPersonasRepository
{
    Task<int> Insertar(Persona persona);
    Task<int> Actualizar(Persona persona);
    Task<bool> Eliminar(int id);
    Task<(IEnumerable<Persona>, int)> ObtenerPersonas(string? filtro, int pageNumber, int pageSize);
}
