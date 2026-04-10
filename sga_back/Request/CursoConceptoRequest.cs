using FluentValidation;

namespace sga_back.Request;

public class CursoConceptoRequest
{
    public string TipoConcepto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public List<CursoConceptoVencimientoRequest> Vencimientos { get; set; } = new();
}
public class CursoConceptoRequestValidator : AbstractValidator<CursoConceptoRequest>
{
    private static readonly string[] TiposValidos = new[]
    {
        "Matricula", "Cuota", "Practica", "DerechoExamen"
    };

    public CursoConceptoRequestValidator()
    {
        RuleFor(x => x.TipoConcepto)
            .NotEmpty()
            .WithMessage("El tipo de concepto es obligatorio.")
            .Must(x => TiposValidos.Contains(x))
            .WithMessage("El tipo de concepto no es válido. Valores permitidos: Matricula, Cuota, Practica, DerechoExamen.");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("La descripción del concepto es obligatoria.")
            .MaximumLength(150)
            .WithMessage("La descripción no puede superar los 150 caracteres.");

    }
}