using AutoMapper;
using sga_back.Models;
using sga_back.Request;
using sga_back.Response;

namespace sga_back.Mappings;

public class InscripcionesAutomapping : Profile
{
    public InscripcionesAutomapping()
    {
        _ = CreateMap<InscripcionRequest, Inscripcion>();
        _ = CreateMap<Inscripcion, InscripcionResponse>();
    }
}
