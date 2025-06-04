namespace sga_back.DTOs;

public class InscripcionDetalleDto
{
    public int IdInscripcion { get; set; }
    public int IdPersona { get; set; }
    public string NombreEstudiante { get; set; } = string.Empty;
    public int IdCurso { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public DateTime FechaInscripcion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal MontoDescuento { get; set; }
    public string MotivoDescuento { get; set; } = string.Empty;
    public decimal MontoDescPractica { get; set; }
    public string MotivoDescPractica { get; set; } = string.Empty;
    public decimal MontoDescMatricula { get; set; }
    public string MotivoDescMatricula { get; set; } = string.Empty;
}
