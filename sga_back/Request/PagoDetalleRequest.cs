using FluentValidation;

namespace sga_back.Request;

public class PagoDetalleRequest
{
    public string Concepto { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string TipoPago { get; set; }
    public string? Referencia { get; set; }
    public string? VoucherNumero { get; set; }
}
public class PagoDetalleRequestValidator : AbstractValidator<PagoDetalleRequest>
{
    public PagoDetalleRequestValidator()
    {
        RuleFor(p => p.Concepto).NotEmpty().WithMessage("El concepto es obligatorio.");
        RuleFor(p => p.Monto).GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");
        RuleFor(p => p.FechaVencimiento).NotEmpty().WithMessage("La fecha de vencimiento es obligatoria.");
    }
}