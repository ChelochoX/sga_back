using AutoMapper;
using sga_back.Models;
using sga_back.Request;

namespace sga_back.Mappings;

public class PersonasAutomapping : Profile
{
    public PersonasAutomapping()
    {
        _ = CreateMap<PersonaRequest, Persona>();
    }
}
