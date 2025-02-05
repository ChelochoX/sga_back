namespace sga_back.Models;

public class Curso
{
    public int IdCurso { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int Duracion { get; set; }
    public string UnidadDuracion { get; set; }
    public decimal Costo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}
