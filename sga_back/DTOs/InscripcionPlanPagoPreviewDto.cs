namespace sga_back.DTOs;

public class InscripcionPlanPagoPreviewDto
{
    public int IdCurso { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int CantidadPagos { get; set; }
    public decimal DescuentoAplicado { get; set; }
    public List<InscripcionPlanPagoDetalleDto> Detalles { get; set; } = new();
}
public class InscripcionPlanPagoDetalleDto
{
    public string TipoConcepto { get; set; } = string.Empty;
    public string Concepto { get; set; } = string.Empty;
    public decimal MontoOriginal { get; set; }
    public decimal DescuentoAplicado { get; set; }
    public decimal MontoFinal { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int NroOrden { get; set; }
}