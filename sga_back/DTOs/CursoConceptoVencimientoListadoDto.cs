namespace sga_back.DTOs;

public class CursoConceptoVencimientoListadoDto
{
    public int IdCursoConceptoVencimiento { get; set; }
    public int IdCursoConcepto { get; set; }
    public int NroOrden { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
