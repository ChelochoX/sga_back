namespace sga_back.DTOs;

public class CursoConceptoVencimientoDto
{
    public int IdCursoConceptoVencimiento { get; set; }
    public int NroOrden { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}
