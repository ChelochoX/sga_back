namespace sga_back.Wrappers;

public class Datos<T>
{
    public required T Items { get; set; }
    public int TotalRegistros { get; set; }

    public static implicit operator Datos<T>(string v)
    {
        throw new NotImplementedException();
    }
}
