namespace sga_back.DTOs;

public class PagoCabeceraDto
{
    public int IdPago { get; set; }
    public int IdInscripcion { get; set; }
    public string NombreEstudiante { get; set; }
    public string DireccionEstudiante { get; set; }
    public string RucEstudiante { get; set; }
    public string TelefonoEstudiante { get; set; }
    public string NombreCurso { get; set; } // si querés traer el curso
    public decimal? DeudaTotal { get; set; }
    public string TipoCuenta { get; set; }
    public decimal? DescuentoCabecera { get; set; }
    public string Observacion { get; set; }
    public List<PagoDetalleDto> Detalles { get; set; } = new();
}
