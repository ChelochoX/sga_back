using AutoMapper;
using sga_back.Models;
using sga_back.Request;

namespace sga_back.Mappings;

public class UsuarioAutomapping : Profile
{
    public UsuarioAutomapping()
    {
        _ = CreateMap<UsuarioRequest, Usuario>()
            .ForMember(dest => dest.ContrasenaHash, opt => opt.Ignore());  // No copiamos la contraseña como hash directamente.
    }
}
