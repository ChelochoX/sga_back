namespace sga_back.Request;

public class InscripcionFiltroRequest
{
    public string? Alumno { get; set; }
    public string? CursoNombre { get; set; }
    public string? FechaDesde { get; set; }
    public string? FechaHasta { get; set; }
}
