namespace sga_back.Models;

public class Inscripcion
{
    public int IdInscripcion { get; set; }
    public int IdPersona { get; set; }
    public int IdCurso { get; set; }
    public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Activa";
    public decimal MontoDescuento { get; set; }
    public string MotivoDescuento { get; set; } = string.Empty;
    public decimal MontoDescuentoPractica { get; set; }
    public string MotivoDescuentoPractica { get; set; } = string.Empty;
}
