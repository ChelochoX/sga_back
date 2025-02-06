using FluentValidation;

namespace sga_back.Request;

public class UsuarioUpdateRequest
{
    public required string NombreUsuario { get; set; }
    public required string NuevaContrasena { get; set; }
}

public class UsuarioUpdateRequestValidator : AbstractValidator<UsuarioUpdateRequest>
{
    public UsuarioUpdateRequestValidator()
    {
        _ = RuleFor(u => u.NombreUsuario)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de usuario no puede superar los 100 caracteres.");

        _ = RuleFor(u => u.NuevaContrasena)
            .NotEmpty().WithMessage("La nueva contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La nueva contraseña debe contener al menos una letra mayúscula.")
            .Matches("[a-z]").WithMessage("La nueva contraseña debe contener al menos una letra minúscula.")
            .Matches("[0-9]").WithMessage("La nueva contraseña debe contener al menos un número.");
    }
}