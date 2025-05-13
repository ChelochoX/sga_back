using FluentValidation;

namespace sga_back.Request;

public class UsuarioNameUpdateRequest
{
    public int IdUsuario { get; set; }
    public string? NombreUsuario { get; set; } = string.Empty;
    public string? Estado { get; set; }
    public DateTime FechaModificacion { get; set; }
}

public class UsuarioNameUpdateRequestValidator : AbstractValidator<UsuarioNameUpdateRequest>
{
    public UsuarioNameUpdateRequestValidator()
    {
        RuleFor(x => x.IdUsuario)
            .GreaterThan(0).WithMessage("El ID del usuario debe ser mayor que 0.");

        RuleFor(x => x.FechaModificacion)
            .NotEmpty().WithMessage("La fecha de modificación es obligatoria.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("La fecha de modificación no puede ser futura.");
    }
}