using FluentValidation;

namespace sga_back.Request;

public class UsuarioRequest
{
    public int IdPersona { get; set; }
    public required string NombreUsuario { get; set; }
    public required string Contrasena { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
public class UsuarioRequestValidator : AbstractValidator<UsuarioRequest>
{
    public UsuarioRequestValidator()
    {
        _ = RuleFor(u => u.NombreUsuario)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de usuario no puede superar los 100 caracteres.");

        _ = RuleFor(u => u.Contrasena)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número.");

        _ = RuleFor(u => u.Estado)
            .Must(e => new[] { "Activo", "Inactivo", "Suspendido" }.Contains(e))
            .When(u => !string.IsNullOrEmpty(u.Estado))
            .WithMessage("El estado no es válido.");
    }
}