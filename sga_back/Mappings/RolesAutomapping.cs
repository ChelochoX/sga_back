using AutoMapper;
using sga_back.Models;
using sga_back.Request;

namespace sga_back.Mappings;

public class RolesAutomapping : Profile
{
    public RolesAutomapping()
    {
        _ = CreateMap<RoleRequest, Rol>();

    }
}
