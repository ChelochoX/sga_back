using FluentValidation;

namespace sga_back.Request;

public class ObtenerCursosRequest
{
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
}

public class ObtenerCursosRequestValidator : AbstractValidator<ObtenerCursosRequest>
{
    public ObtenerCursosRequestValidator()
    {
        RuleFor(x => x.FechaInicio)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria.");

        RuleFor(x => x.FechaFin)
            .GreaterThanOrEqualTo(x => x.FechaInicio)
            .When(x => x.FechaFin.HasValue)
            .WithMessage("La fecha de fin debe ser igual o posterior a la fecha de inicio.");
    }
}
