using AutoMapper;
using sga_back.DTOs;
using sga_back.Models;
using sga_back.Request;

namespace sga_back.Mappings;

public class CursosAutomapping : Profile
{
    public CursosAutomapping()
    {
        CreateMap<CursoRequest, Curso>();
        CreateMap<CursoConceptoRequest, CursoConcepto>();
        CreateMap<CursoConceptoVencimientoRequest, CursoConceptoVencimiento>();

        CreateMap<Curso, CursoDetalleDto>();
        CreateMap<CursoConcepto, CursoConceptoDto>();
        CreateMap<CursoConceptoVencimiento, CursoConceptoVencimientoDto>();
    }
}