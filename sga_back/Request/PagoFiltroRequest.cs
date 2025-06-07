namespace sga_back.Request;

public class PagoFiltroRequest
{
    public string NombreEstudiante { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
