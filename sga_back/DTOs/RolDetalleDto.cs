namespace sga_back.DTOs;

public class RolDetalleDto
{
    public int IdRol { get; set; }
    public string NombreRol { get; set; } = "";
    public List<EntidadDetalleDto> Entidades { get; set; } = new();
}

public class EntidadDetalleDto
{
    public int IdEntidad { get; set; }
    public string NombreEntidad { get; set; } = "";
    public List<string> Acciones { get; set; } = new();
}
