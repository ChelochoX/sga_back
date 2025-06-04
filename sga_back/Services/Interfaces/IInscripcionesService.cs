using sga_back.DTOs;
using sga_back.Request;
using sga_back.Response;

namespace sga_back.Services.Interfaces;

public interface IInscripcionesService
{
    Task<int> Insertar(InscripcionRequest request);
    Task<int> Actualizar(int idInscripcion, InscripcionRequest request);
    Task<bool> Eliminar(int idInscripcion);
    Task<InscripcionResponse?> ObtenerPorId(int idInscripcion);
    Task<IEnumerable<InscripcionResponse>> ObtenerTodas();
    Task<IEnumerable<EstudianteDto>> ObtenerEstudiantes(string? search);
    Task<IEnumerable<CursosInscripcionDto>> ObtenerCursos(string? search);
}
