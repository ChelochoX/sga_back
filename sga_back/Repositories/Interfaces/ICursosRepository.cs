using sga_back.DTOs;
using sga_back.Models;
using sga_back.Request;

namespace sga_back.Repositories.Interfaces;

public interface ICursosRepository
{
    Task<int> Insertar(Curso curso);
    Task<int> Actualizar(Curso curso);
    Task<bool> Eliminar(int id);
    Task<Curso?> ObtenerPorId(int idCurso);
    Task<IEnumerable<CursoDto>> ObtenerCursosPorFecha(ObtenerCursosRequest request);
}
