namespace sga_back.Models;

public class CursoConceptoDetalle
{
    public int IdCursoConceptoDetalle { get; set; }
    public int IdCursoConcepto { get; set; }
    public int NumeroOrden { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
