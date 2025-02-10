using FluentValidation;

namespace sga_back.Request;

public class InscripcionRequest
{
    public int IdPersona { get; set; }
    public int IdCurso { get; set; }
    public string? Estado { get; set; } = "Activa";
    public decimal MontoDescuento { get; set; }
    public required string MotivoDescuento { get; set; }
    public decimal MontoDescuentoPractica { get; set; }
    public string? MotivoDescuentoPractica { get; set; }
}
public class InscripcionRequestValidator : AbstractValidator<InscripcionRequest>
{
    public InscripcionRequestValidator()
    {
        _ = RuleFor(i => i.IdPersona)
             .GreaterThan(0).WithMessage("El ID de persona es obligatorio y debe ser mayor que 0.");

        _ = RuleFor(i => i.IdCurso)
            .GreaterThan(0).WithMessage("El ID de curso es obligatorio y debe ser mayor que 0.");

        _ = RuleFor(i => i.Estado)
            .Must(e => new[] { "Activa", "Inactiva", "Cancelada" }.Contains(e))
            .WithMessage("El estado no es válido.");

        _ = RuleFor(i => i.MontoDescuento)
            .GreaterThanOrEqualTo(0).WithMessage("El monto de descuento no puede ser negativo.");

        _ = RuleFor(i => i.MotivoDescuento)
            .NotEmpty().WithMessage("El motivo del descuento es obligatorio.")
            .MaximumLength(100).WithMessage("El motivo del descuento no puede superar los 100 caracteres.");

        _ = RuleFor(i => i.MontoDescuentoPractica)
            .GreaterThanOrEqualTo(0).WithMessage("El monto de descuento de práctica no puede ser negativo.");

    }
}