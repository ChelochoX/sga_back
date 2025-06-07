namespace sga_back.DTOs;

public class PagoDetalleDto
{
    public int IdPago { get; set; }
    public int IdInscripcion { get; set; }
    public int IdPersona { get; set; }
    public string NombreEstudiante { get; set; }
    public decimal DeudaTotal { get; set; }
    public string TipoCuenta { get; set; }
    public decimal DescuentoCabecera { get; set; }
    public string Observacion { get; set; }
    public int? IdDetallePago { get; set; }
    public string Concepto { get; set; }
    public decimal? Monto { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaPago { get; set; }
    public string TipoPago { get; set; }
    public string Referencia { get; set; }
    public string VoucherNumero { get; set; }
    public string Estado { get; set; }
}
