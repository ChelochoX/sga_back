namespace sga_back.Request;

public class UsuarioNameUpdateRequest
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Estado { get; set; }
    public DateTime FechaModificacion { get; set; }
}
