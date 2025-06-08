namespace sga_back.Request;

public class FacturaContadoRequest
{
    public string Sucursal { get; set; }
    public string Caja { get; set; }
    public string Numero { get; set; }
    public string RucCliente { get; set; }
    public string NombreCliente { get; set; }
    public decimal TotalFactura { get; set; }
    public string Observacion { get; set; }
    public string UsuarioRegistro { get; set; }
    public List<FacturaDetalleRequest> Detalles { get; set; }
}
