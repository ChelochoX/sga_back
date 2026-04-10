namespace sga_back.DTOs;

public class CursoConceptoDto
{
    public int IdCursoConcepto { get; set; }
    public string TipoConcepto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }

    public List<CursoConceptoVencimientoDto> Vencimientos { get; set; } = new();
}
