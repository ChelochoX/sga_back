using FluentValidation;

namespace sga_back.Request;

public class InscripcionRequest
{
    public int IdPersona { get; set; }
    public int IdCurso { get; set; }
    public string? Estado { get; set; } = "Activa";
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
    }
}