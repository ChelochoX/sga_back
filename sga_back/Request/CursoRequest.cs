using FluentValidation;

namespace sga_back.Request;

public class CursoRequest
{
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public required int Duracion { get; set; }
    public required string UnidadDuracion { get; set; }
    public required int CantidadCuota { get; set; }
    public required decimal MontoCuota { get; set; }
    public required char TienePractica { get; set; }
    public required decimal CostoPractica { get; set; }
    public required DateTime FechaInicio { get; set; }
    public required DateTime FechaFin { get; set; }
}
public class CursoRequestValidator : AbstractValidator<CursoRequest>
{
    public CursoRequestValidator()
    {
        _ = RuleFor(c => c.Nombre)
            .NotEmpty().WithMessage("El nombre del curso es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

        _ = RuleFor(c => c.Duracion)
            .GreaterThan(0).WithMessage("La duración del curso debe ser mayor a 0.");

        _ = RuleFor(c => c.UnidadDuracion)
            .NotEmpty().WithMessage("La unidad de duración es obligatoria.")
            .Must(u => new[] { "Horas", "Dias", "Semanas", "Meses" }.Contains(u))
            .WithMessage("La unidad de duración debe ser 'Horas', 'Dias', 'Semanas' o 'Meses'.");

        _ = RuleFor(c => c.CantidadCuota)
            .GreaterThan(0).WithMessage("La cantidad de cuotas debe ser mayor a 0.");

        _ = RuleFor(c => c.MontoCuota)
            .GreaterThan(0).WithMessage("El monto de la cuota debe ser mayor a 0.");

        _ = RuleFor(c => c.TienePractica)
            .NotEmpty().WithMessage("Debe especificar si el curso tiene práctica.");

        _ = RuleFor(c => c.CostoPractica)
            .GreaterThanOrEqualTo(0).WithMessage("El costo de la práctica no puede ser negativo.");

        _ = RuleFor(c => c.FechaInicio)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria.");

        _ = RuleFor(c => c.FechaFin)
            .NotEmpty().WithMessage("La fecha de fin es obligatoria.")
            .GreaterThan(c => c.FechaInicio).WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");
    }
}