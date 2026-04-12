using sga_back.DTOs;
using sga_back.Request;

namespace sga_back.Services.Interfaces;

public interface IDocumentosFiscalesConfigService
{
    Task<int> Insertar(DocumentoFiscalConfigRequest request);
    Task<bool> Actualizar(int id, DocumentoFiscalConfigRequest request);
    Task<bool> Eliminar(int id);
    Task<DocumentoFiscalConfigDto?> ObtenerPorId(int id);
    Task<IEnumerable<DocumentoFiscalConfigDto>> ObtenerTodas(DocumentoFiscalConfigFiltroRequest filtro);
    Task<IEnumerable<TipoDocumentoFiscalDto>> ObtenerTiposDocumento();
}
