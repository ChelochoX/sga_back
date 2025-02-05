using AutoMapper;
using sga_back.Models;
using sga_back.Request;

namespace sga_back.Mappings;

public class CursosAutomapping : Profile
{
    public CursosAutomapping()
    {
        _ = CreateMap<CursoRequest, Curso>();
    }
}