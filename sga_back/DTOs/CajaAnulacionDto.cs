namespace sga_back.DTOs;

public class CajaAnulacionDto
{
    public int IdAnulacion { get; set; }
    public int IdMovimiento { get; set; }
    public string Motivo { get; set; }
    public string UsuarioAnulacion { get; set; }
    public DateTime FechaAnulacion { get; set; }
}
