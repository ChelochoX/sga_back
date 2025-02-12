namespace sga_back.Response;

public class PagoDetalleResponse
{
    public int IdDetalle { get; set; }
    public string Concepto { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaPago { get; set; }
    public string TipoPago { get; set; }
    public string? Referencia { get; set; }
    public string? VoucherNumero { get; set; }
    public string Estado { get; set; }
}
