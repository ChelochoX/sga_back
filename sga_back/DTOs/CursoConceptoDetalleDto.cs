namespace sga_back.DTOs;

public class CursoConceptoDetalleDto
{
    public int IdCursoConceptoDetalle { get; set; }
    public int NumeroOrden { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public bool Activo { get; set; }
}
