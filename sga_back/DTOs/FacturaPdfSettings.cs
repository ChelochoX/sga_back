namespace sga_back.DTOs;

public class FacturaPdfSettings
{
    public string RucEmisor { get; set; } = string.Empty;
    public string Timbrado { get; set; } = string.Empty;
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public string TipoTransaccion { get; set; } = "Venta de servicios educativos";
}
