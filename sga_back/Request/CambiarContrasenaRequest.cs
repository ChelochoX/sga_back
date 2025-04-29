namespace sga_back.Request;

public class CambiarContrasenaRequest
{
    public string Usuario { get; set; }
    public int? IdUsuario { get; set; }
    public string NuevaContrasena { get; set; } = string.Empty;
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
