using sga_back.Models;

namespace sga_back.Repositories.Interfaces;

public interface ICursosRepository
{
    Task<int> Insertar(Curso curso);
    Task<int> Actualizar(Curso curso);
    Task<bool> Eliminar(int id);
}
