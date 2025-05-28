namespace sga_back.DTOs;

public class CursoDto
{
    public int IdCurso { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int Duracion { get; set; }
    public string UnidadDuracion { get; set; }
    public int CantidadCuota { get; set; }
    public decimal MontoCuota { get; set; }
    public string TienePractica { get; set; }
    public decimal CostoPractica { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal MontoMatricula { get; set; }
}
