namespace sga_back.DTOs;

public class ActualizarRolesRequest
{
    public string NombreUsuario { get; set; }
    public List<int> IdsRoles { get; set; }
}
