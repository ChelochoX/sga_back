namespace sga_back.DTOs;

public class DocumentoFiscalConfigDto
{
    public int Id { get; set; }
    public string TipoDocumento { get; set; }
    public string Sucursal { get; set; }
    public string PuntoExpedicion { get; set; }
    public string Timbrado { get; set; }
    public int NumeroActual { get; set; }
    public int NumeroInicio { get; set; }
    public int NumeroFin { get; set; }
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public string RucEmisor { get; set; }
    public string RazonSocialEmisor { get; set; }
    public string DireccionEmisor { get; set; }
}
