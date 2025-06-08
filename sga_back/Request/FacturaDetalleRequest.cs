namespace sga_back.Request;

public class FacturaDetalleRequest
{
    public string Concepto { get; set; }
    public decimal Monto { get; set; }
    public int IdPago { get; set; }
    public int IdDetallePago { get; set; }
    public string Observacion { get; set; }
}
