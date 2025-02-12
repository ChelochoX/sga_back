using FluentValidation;

namespace sga_back.Request;

public class PagoRequest
{
    public int IdInscripcion { get; set; }
    public decimal Total { get; set; }
    public string TipoCuenta { get; set; }
    public decimal Descuento { get; set; }
    public string Observacion { get; set; }
    public List<PagoDetalleRequest> Detalles { get; set; } = new();
}
public class PagoEncabezadoRequestValidator : AbstractValidator<PagoRequest>
{
    public PagoEncabezadoRequestValidator()
    {
        RuleFor(p => p.IdInscripcion).GreaterThan(0).WithMessage("El ID de inscripción es obligatorio.");
        RuleFor(p => p.Total).GreaterThan(0).WithMessage("El total debe ser mayor a 0.");
        RuleFor(p => p.TipoCuenta).NotEmpty().WithMessage("El tipo de cuenta es obligatorio.");
    }
}