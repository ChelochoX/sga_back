namespace sga_back.Models;

public class Usuario
{
    public int IdUsuario { get; set; }
    public int IdPersona { get; set; }
    public int IdRol { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public string? Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string ContrasenaTemporal { get; set; }
    public bool RequiereCambioContrasena { get; set; }
}
