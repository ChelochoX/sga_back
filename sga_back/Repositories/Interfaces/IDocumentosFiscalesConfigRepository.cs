using sga_back.DTOs;
using sga_back.Request;

namespace sga_back.Repositories.Interfaces;

public interface IDocumentosFiscalesConfigRepository
{
    Task<int> Insertar(DocumentoFiscalConfigRequest request);
    Task<bool> Actualizar(int id, DocumentoFiscalConfigRequest request);
    Task<bool> Eliminar(int id);
    Task<DocumentoFiscalConfigDto?> ObtenerPorId(int id);
    Task<IEnumerable<DocumentoFiscalConfigDto>> ObtenerTodas(DocumentoFiscalConfigFiltroRequest filtro);
    Task<IEnumerable<TipoDocumentoFiscalDto>> ObtenerTiposDocumento();
    Task<bool> ExisteDuplicadoActivo(
        int tipoDocumentoId,
        string sucursal,
        string puntoExpedicion,
        string conceptoDocumento,
        int? idExcluir = null);
}
