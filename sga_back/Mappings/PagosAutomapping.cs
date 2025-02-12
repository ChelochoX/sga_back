using AutoMapper;
using sga_back.Models;
using sga_back.Request;
using sga_back.Response;

namespace sga_back.Mappings;

public class PagosAutomapping : Profile
{
    public PagosAutomapping()
    {
        _ = CreateMap<PagoRequest, PagoEncabezado>();
        _ = CreateMap<PagoDetalleRequest, PagoDetalle>();

        _ = CreateMap<PagoEncabezado, PagoResponse>();
        _ = CreateMap<PagoDetalle, PagoDetalleResponse>();
    }
}
