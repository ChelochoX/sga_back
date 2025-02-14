namespace sga_back.Models;

public class PagoDetalle
{
    public int IdDetalle { get; set; }
    public int IdPago { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaPago { get; set; }
    public string? TipoPago { get; set; }
    public string? Referencia { get; set; }
    public string? VoucherNumero { get; set; }
    public string? Estado { get; set; }
}
