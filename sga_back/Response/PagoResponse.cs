namespace sga_back.Response;

public class PagoResponse
{
    public int IdPago { get; set; }
    public int IdInscripcion { get; set; }
    public decimal Total { get; set; }
    public string TipoCuenta { get; set; }
    public decimal Descuento { get; set; }
    public string Observacion { get; set; }
    public string? FacturaNumero { get; set; }
    public string? ReciboNumero { get; set; }
    public List<PagoDetalleResponse> Detalles { get; set; } = new();
}
