using FluentValidation;

namespace sga_back.Request;

public class DocumentoFiscalConfigRequest
{
    public int TipoDocumentoId { get; set; }
    public string Sucursal { get; set; } = string.Empty;
    public string PuntoExpedicion { get; set; } = string.Empty;
    public string Timbrado { get; set; } = string.Empty;
    public int NumeroActual { get; set; }
    public int NumeroInicio { get; set; }
    public int NumeroFin { get; set; }
    public DateTime VigenciaDesde { get; set; }
    public DateTime VigenciaHasta { get; set; }
    public string RucEmisor { get; set; } = string.Empty;
    public string RazonSocialEmisor { get; set; } = string.Empty;
    public string DireccionEmisor { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string ConceptoDocumento { get; set; } = string.Empty;
}
public class DocumentoFiscalConfigRequestValidator : AbstractValidator<DocumentoFiscalConfigRequest>
{
    public DocumentoFiscalConfigRequestValidator()
    {
        RuleFor(x => x.TipoDocumentoId)
            .GreaterThan(0).WithMessage("El tipo de documento es obligatorio.");

        RuleFor(x => x.Sucursal)
            .NotEmpty().WithMessage("La sucursal es obligatoria.")
            .MaximumLength(10);

        RuleFor(x => x.PuntoExpedicion)
            .NotEmpty().WithMessage("El punto de expedición es obligatorio.")
            .MaximumLength(10);

        RuleFor(x => x.Timbrado)
            .NotEmpty().WithMessage("El timbrado es obligatorio.")
            .MaximumLength(50);

        RuleFor(x => x.NumeroInicio)
            .GreaterThan(0).WithMessage("El número inicio debe ser mayor a cero.");

        RuleFor(x => x.NumeroActual)
            .GreaterThan(0).WithMessage("El número actual debe ser mayor a cero.");

        RuleFor(x => x.NumeroFin)
            .GreaterThan(0).WithMessage("El número fin debe ser mayor a cero.");

        RuleFor(x => x)
            .Must(x => x.NumeroInicio <= x.NumeroActual)
            .WithMessage("El número actual no puede ser menor al número inicio.");

        RuleFor(x => x)
            .Must(x => x.NumeroActual <= x.NumeroFin)
            .WithMessage("El número actual no puede ser mayor al número fin.");

        RuleFor(x => x)
            .Must(x => x.NumeroInicio <= x.NumeroFin)
            .WithMessage("El número inicio no puede ser mayor al número fin.");

        RuleFor(x => x.VigenciaDesde)
            .NotEmpty().WithMessage("La vigencia desde es obligatoria.");

        RuleFor(x => x.VigenciaHasta)
            .NotEmpty().WithMessage("La vigencia hasta es obligatoria.");

        RuleFor(x => x)
            .Must(x => x.VigenciaDesde.Date <= x.VigenciaHasta.Date)
            .WithMessage("La vigencia desde no puede ser mayor que la vigencia hasta.");

        RuleFor(x => x.RucEmisor)
            .NotEmpty().WithMessage("El RUC del emisor es obligatorio.")
            .MaximumLength(30);

        RuleFor(x => x.RazonSocialEmisor)
            .NotEmpty().WithMessage("La razón social del emisor es obligatoria.")
            .MaximumLength(200);

        RuleFor(x => x.DireccionEmisor)
            .NotEmpty().WithMessage("La dirección del emisor es obligatoria.")
            .MaximumLength(300);

        RuleFor(x => x.ConceptoDocumento)
            .NotEmpty().WithMessage("El concepto del documento es obligatorio.")
            .MaximumLength(100);
    }
}