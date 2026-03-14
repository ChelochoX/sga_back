namespace sga_back.Services.Interfaces;

public interface IFacturaPdfService
{
    Task<byte[]> GenerarPdfPorFactura(int idFactura);
    Task<byte[]> GenerarPdfPorMovimiento(int idMovimiento);
    Task<byte[]> GenerarPdfPorAnulacion(int idAnulacion);
}
