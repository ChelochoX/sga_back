namespace sga_back.Models;

public class CursoConcepto
{
    public int IdCursoConcepto { get; set; }
    public int IdCurso { get; set; }
    public string TipoConcepto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public List<CursoConceptoVencimiento> Vencimientos { get; set; } = new();
}
