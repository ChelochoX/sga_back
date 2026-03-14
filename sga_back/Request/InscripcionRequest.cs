using FluentValidation;

namespace sga_back.Request;

public class InscripcionRequest
{
    public int IdPersona { get; set; }
    public int IdCurso { get; set; }
    public string Estado { get; set; } = "Activa";
    public DateTime? FechaInscripcion { get; set; }

    public decimal MontoDescuento { get; set; } = 0;
    public string MotivoDescuento { get; set; } = string.Empty;

    public decimal MontoDescuentoPractica { get; set; } = 0;
    public string MotivoDescuentoPractica { get; set; } = string.Empty;

    public decimal MontoDescuentoMatricula { get; set; } = 0;
    public string MotivoDescuentoMatricula { get; set; } = string.Empty;
}

public class InscripcionRequestValidator : AbstractValidator<InscripcionRequest>
{
    public InscripcionRequestValidator()
    {
        _ = RuleFor(i => i.IdPersona)
            .GreaterThan(0)
            .WithMessage("El ID de persona es obligatorio y debe ser mayor que 0.");

        _ = RuleFor(i => i.IdCurso)
            .GreaterThan(0)
            .WithMessage("El ID de curso es obligatorio y debe ser mayor que 0.");

        _ = RuleFor(i => i.Estado)
            .Must(e => new[] { "Activa", "Inactiva", "Cancelada" }.Contains(e))
            .WithMessage("El estado no es válido.");

        _ = RuleFor(i => i.MontoDescuento)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El monto de descuento no puede ser negativo.");

        _ = RuleFor(i => i.MontoDescuentoPractica)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El monto de descuento de práctica no puede ser negativo.");

        _ = RuleFor(i => i.MontoDescuentoMatricula)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El monto de descuento de matrícula no puede ser negativo.");

        _ = RuleFor(i => i.MotivoDescuento)
            .NotEmpty()
            .WithMessage("El motivo del descuento es obligatorio cuando el monto es mayor a 0.")
            .MaximumLength(100)
            .WithMessage("El motivo del descuento no puede superar los 100 caracteres.")
            .When(i => i.MontoDescuento > 0);

        _ = RuleFor(i => i.MotivoDescuentoPractica)
            .NotEmpty()
            .WithMessage("El motivo del descuento de práctica es obligatorio cuando el monto es mayor a 0.")
            .MaximumLength(100)
            .WithMessage("El motivo del descuento de práctica no puede superar los 100 caracteres.")
            .When(i => i.MontoDescuentoPractica > 0);

        _ = RuleFor(i => i.MotivoDescuentoMatricula)
            .NotEmpty()
            .WithMessage("El motivo del descuento de matrícula es obligatorio cuando el monto es mayor a 0.")
            .MaximumLength(100)
            .WithMessage("El motivo del descuento de matrícula no puede superar los 100 caracteres.")
            .When(i => i.MontoDescuentoMatricula > 0);
    }
}
