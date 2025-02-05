using sga_back.Request;

namespace sga_back.Services.Interfaces;

public interface ICursosService
{
    Task<int> Insertar(CursoRequest request);
    Task<int> Actualizar(int id, CursoRequest request);
    Task<bool> Eliminar(int id);
}
