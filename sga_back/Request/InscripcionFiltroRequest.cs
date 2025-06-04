namespace sga_back.Request;

public class InscripcionFiltroRequest
{
    public string? Alumno { get; set; }
    public string? CursoNombre { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}
