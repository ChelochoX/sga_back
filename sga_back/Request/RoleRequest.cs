using FluentValidation;

namespace sga_back.Request;

public class RoleRequest
{
    public required string NombreRol { get; set; }
}
public class RoleRequestValidator : AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        _ = RuleFor(r => r.NombreRol)
            .NotEmpty().WithMessage("El nombre del rol es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre del rol no puede superar los 50 caracteres.");
    }
}