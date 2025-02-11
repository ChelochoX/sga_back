namespace sga_back.Models;

public class PagoEncabezado
{
    public int IdPago { get; set; }
    public int IdInscripcion { get; set; }
    public decimal Total { get; set; }
    public string TipoCuenta { get; set; } = "Crédito";
    public decimal Descuento { get; set; }
    public string? Observacion { get; set; }
    public string? FacturaNumero { get; set; }
    public string? ReciboNumero { get; set; }
}
