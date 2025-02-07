namespace sga_back.Response;

public class InscripcionResponse
{
    public int IdInscripcion { get; set; }
    public int IdPersona { get; set; }
    public int IdCurso { get; set; }
    public DateTime FechaInscripcion { get; set; }
    public string Estado { get; set; } = "Activa";
}
