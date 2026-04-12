namespace sga_back.DTOs;

public class CursoConceptoListadoDto
{
    public int IdCursoConcepto { get; set; }
    public int IdCurso { get; set; }
    public string TipoConcepto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }

    public List<CursoConceptoVencimientoListadoDto> Vencimientos { get; set; } = new();
}
