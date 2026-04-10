using FluentValidation;

namespace sga_back.Request;

public class CursoConceptoDetalleRequest
{
    public int NumeroOrden { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
}
public class CursoConceptoDetalleRequestValidator : AbstractValidator<CursoConceptoDetalleRequest>
{
    public CursoConceptoDetalleRequestValidator()
    {
        RuleFor(x => x.NumeroOrden)
            .GreaterThan(0)
            .WithMessage("El número de orden debe ser mayor a 0.");

        RuleFor(x => x.Concepto)
            .NotEmpty()
            .WithMessage("El concepto es obligatorio.")
            .MaximumLength(300)
            .WithMessage("El concepto no puede superar los 150 caracteres.");

        RuleFor(x => x.Monto)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El monto no puede ser negativo.");

        RuleFor(x => x.FechaVencimiento)
            .NotEmpty()
            .WithMessage("La fecha de vencimiento es obligatoria.");
    }
}