namespace sga_back.Models;

public class Curso
{
    public int IdCurso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Duracion { get; set; }
    public string UnidadDuracion { get; set; } = string.Empty;
    public int CantidadCuota { get; set; }
    public decimal MontoMatricula { get; set; }  // NUEVO: Monto de matrícula
    public decimal MontoCuota { get; set; }
    public char TienePractica { get; set; }
    public decimal CostoPractica { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activo { get; set; }
}
