namespace sga_back.Request;

public class CursoConceptoVencimientoRequest
{
    public int NroOrden { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}
