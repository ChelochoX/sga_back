namespace sga_back.Models;

public class Persona
{
    public int IdPersona { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string Ruc { get; set; } = string.Empty;
    public int DigitoVerificador { get; set; }
}
