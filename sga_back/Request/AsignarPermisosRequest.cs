namespace sga_back.Request;

public class AsignarPermisosRequest
{
    public int IdRol { get; set; }
    public List<(int idEntidad, int idRecurso)> Permisos { get; set; }
}
