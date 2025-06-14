namespace sga_back.Models;

public class CajaAnulacion
{
    public int IdAnulacion { get; set; }
    public int IdMovimiento { get; set; }
    public string Motivo { get; set; }
    public string UsuarioAnulacion { get; set; }
    public DateTime FechaAnulacion { get; set; }
}
