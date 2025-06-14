namespace sga_back.DTOs;

public class CajaMovimientoDto
{
    public int IdMovimiento { get; set; }
    public DateTime Fecha { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
