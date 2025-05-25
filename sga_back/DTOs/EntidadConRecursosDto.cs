namespace sga_back.DTOs;

public class EntidadConRecursosDto
{
    public int IdEntidad { get; set; }
    public string NombreEntidad { get; set; }
    public List<RecursoDto> Recursos { get; set; }
}
