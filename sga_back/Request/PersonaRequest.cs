using FluentValidation;

namespace sga_back.Request;

public class PersonaRequest
{
    public required string Nombres { get; set; }
    public required string Apellidos { get; set; }
    public string? Email { get; set; }
    public required string Telefono { get; set; }
    public required string Direccion { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public required string Cedula { get; set; }
    public required string Ruc { get; set; }
    public int DigitoVerificador { get; set; }
}

public class PersonaRequestValidator : AbstractValidator<PersonaRequest>
{
    public PersonaRequestValidator()
    {
        _ = RuleFor(p => p.Nombres)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        _ = RuleFor(p => p.Apellidos)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

        _ = RuleFor(p => p.Telefono)
            .MaximumLength(20).WithMessage("El teléfono no puede superar los 20 caracteres.");

        _ = RuleFor(p => p.Cedula)
            .NotEmpty().WithMessage("La cédula es obligatoria.")
            .MaximumLength(15).WithMessage("La cédula no puede superar los 15 caracteres.");

        _ = RuleFor(p => p.FechaNacimiento)
            .NotEmpty().WithMessage("La fecha de nacimiento es obligatoria.")
            .LessThan(DateTime.Now).WithMessage("La fecha de nacimiento debe ser en el pasado.");
    }
}
