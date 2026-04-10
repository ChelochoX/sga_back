using FluentValidation;

namespace sga_back.Request;

public class CursoRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Duracion { get; set; }
    public string UnidadDuracion { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activo { get; set; }

    public List<CursoConceptoRequest> Conceptos { get; set; } = new();
}
public class CursoRequestValidator : AbstractValidator<CursoRequest>
{
    public CursoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre es obligatorio.")
            .MaximumLength(150)
            .WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion))
            .WithMessage("La descripción no puede superar los 1000 caracteres.");

        RuleFor(x => x.Duracion)
            .GreaterThan(0)
            .WithMessage("La duración debe ser mayor a 0.");

        RuleFor(x => x.UnidadDuracion)
            .NotEmpty()
            .WithMessage("La unidad de duración es obligatoria.")
            .Must(x => new[] { "Meses", "Semanas", "Dias", "Horas" }.Contains(x))
            .WithMessage("La unidad de duración debe ser: Meses, Semanas, Dias u Horas.");

        RuleFor(x => x.FechaInicio)
            .NotEmpty()
            .WithMessage("La fecha de inicio es obligatoria.");

        RuleFor(x => x.FechaFin)
            .NotEmpty()
            .WithMessage("La fecha de fin es obligatoria.")
            .GreaterThanOrEqualTo(x => x.FechaInicio)
            .WithMessage("La fecha fin no puede ser menor a la fecha inicio.");

        RuleFor(x => x.Conceptos)
            .NotNull()
            .WithMessage("La lista de conceptos es obligatoria.")
            .Must(x => x.Count > 0)
            .WithMessage("Debe enviar al menos un concepto para el curso.");

        RuleForEach(x => x.Conceptos)
            .SetValidator(new CursoConceptoRequestValidator());

        RuleFor(x => x.Conceptos)
            .Must(x => x.Select(c => c.TipoConcepto).Distinct().Count() == x.Count)
            .WithMessage("No se pueden repetir tipos de concepto dentro del curso.");

        RuleFor(x => x.Conceptos)
            .Must(x => x.Any(c => c.TipoConcepto == "Matricula"))
            .WithMessage("Debe existir al menos un concepto de tipo Matricula.");

        RuleFor(x => x.Conceptos)
            .Must(x => x.Any(c => c.TipoConcepto == "Cuota"))
            .WithMessage("Debe existir al menos un concepto de tipo Cuota.");
    }
}