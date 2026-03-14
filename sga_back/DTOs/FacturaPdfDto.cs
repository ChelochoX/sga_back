namespace sga_back.DTOs;

public class FacturaPdfDto
{
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }

    public string RucEmisor { get; set; } = string.Empty;
    public string Timbrado { get; set; } = string.Empty;
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public string RazonSocialEmisor { get; set; } = string.Empty;
    public string DireccionEmisor { get; set; } = string.Empty;

    public string NombreCliente { get; set; } = string.Empty;
    public string RucCliente { get; set; } = string.Empty;
    public string DireccionCliente { get; set; } = string.Empty;
    public string TelefonoCliente { get; set; } = string.Empty;

    public string CondicionVenta { get; set; } = "Contado";
    public string TipoTransaccion { get; set; } = string.Empty;

    public string EstadoFactura { get; set; } = "Emitido";
    public string Observacion { get; set; } = string.Empty;

    public decimal TotalIva { get; set; }
    public decimal TotalGeneral { get; set; }

    public DateTime? FechaAnulacion { get; set; }
    public string MotivoAnulacion { get; set; } = string.Empty;
    public string UsuarioAnulacion { get; set; } = string.Empty;

    public List<FacturaPdfDetalleDto> Detalles { get; set; } = [];
}

public class FacturaPdfDetalleDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Unidad { get; set; } = "Unidad";
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal Exentas { get; set; }
    public decimal Iva5 { get; set; }
    public decimal Iva10 { get; set; }
    public decimal MontoIva { get; set; }
}