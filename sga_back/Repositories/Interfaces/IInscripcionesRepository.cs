using sga_back.DTOs;
using sga_back.Models;
using sga_back.Request;

namespace sga_back.Repositories.Interfaces;

public interface IInscripcionesRepository
{
    Task<int> Insertar(Inscripcion inscripcion);
    Task<int> Actualizar(Inscripcion inscripcion);
    Task<bool> Eliminar(int idInscripcion);
    Task<Inscripcion?> ObtenerPorId(int idInscripcion);
    Task<IEnumerable<InscripcionDetalleDto>> ObtenerTodas(InscripcionFiltroRequest filtro);
    Task<IEnumerable<EstudianteDto>> ObtenerEstudiantes(string? search);
    Task<IEnumerable<CursosInscripcionDto>> ObtenerCursos(string? search);
}
