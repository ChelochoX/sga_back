namespace sga_back.DTOs;

public class DocumentoFiscalConfigDto
{
    public int Id { get; set; }
    public int TipoDocumentoId { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string Sucursal { get; set; } = string.Empty;
    public string PuntoExpedicion { get; set; } = string.Empty;
    public string Timbrado { get; set; } = string.Empty;
    public int NumeroActual { get; set; }
    public int NumeroInicio { get; set; }
    public int NumeroFin { get; set; }
    public DateTime VigenciaDesde { get; set; }
    public DateTime VigenciaHasta { get; set; }
    public string RucEmisor { get; set; } = string.Empty;
    public string RazonSocialEmisor { get; set; } = string.Empty;
    public string DireccionEmisor { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string ConceptoDocumento { get; set; } = string.Empty;
}
