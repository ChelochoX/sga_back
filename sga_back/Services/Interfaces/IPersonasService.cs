using sga_back.Models;

namespace sga_back.Services.Interfaces;

public interface IPersonasService
{
    Task<int> Insertar(Persona persona);
}
