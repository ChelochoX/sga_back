namespace sga_back.DTOs;

public class PagoDetalleDto
{
    public int IdDetallePago { get; set; }
    public string Concepto { get; set; }
    public decimal Monto { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaPago { get; set; }
    public string TipoPago { get; set; }
    public string Estado { get; set; }
}
