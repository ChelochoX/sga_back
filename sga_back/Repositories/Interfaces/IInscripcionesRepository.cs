using sga_back.Models;

namespace sga_back.Repositories.Interfaces;

public interface IInscripcionesRepository
{
    Task<int> Insertar(Inscripcion inscripcion);
    Task<int> Actualizar(Inscripcion inscripcion);
    Task<bool> Eliminar(int idInscripcion);
    Task<Inscripcion?> ObtenerPorId(int idInscripcion);
    Task<IEnumerable<Inscripcion>> ObtenerTodas();
}
