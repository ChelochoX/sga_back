namespace sga_back.Models;

public class CajaMovimiento
{
    public int IdMovimiento { get; set; }
    public DateTime Fecha { get; set; }
    public string TipoMovimiento { get; set; }
    public decimal Monto { get; set; }
    public string Concepto { get; set; }
    public string Usuario { get; set; }
    public string? Referencia { get; set; }
    public DateTime FechaCreacion { get; set; }
}
