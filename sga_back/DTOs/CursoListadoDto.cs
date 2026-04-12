namespace sga_back.DTOs;

public class CursoListadoDto
{
    public int IdCurso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Duracion { get; set; }
    public string UnidadDuracion { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activo { get; set; }

    public List<CursoConceptoListadoDto> Conceptos { get; set; } = new();
}
