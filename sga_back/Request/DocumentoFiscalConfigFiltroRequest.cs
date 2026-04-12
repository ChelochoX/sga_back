namespace sga_back.Request;

public class DocumentoFiscalConfigFiltroRequest
{
    public string? ConceptoDocumento { get; set; }
    public int? TipoDocumentoId { get; set; }
    public bool? Activo { get; set; }
}
