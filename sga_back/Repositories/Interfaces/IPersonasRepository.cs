using sga_back.Models;

namespace sga_back.Repositories.Interfaces;

public interface IPersonasRepository
{
    Task<int> Insertar(Persona persona);
}
