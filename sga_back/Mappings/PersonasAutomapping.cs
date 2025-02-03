using AutoMapper;
using sga_back.Models;
using sga_back.Request;

namespace sga_back.Mappings;

public class PersonasAutomapping : Profile
{
    protected PersonasAutomapping()
    {
        CreateMap<PersonaRequest, Persona>();
    }
}
