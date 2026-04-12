using AutoMapper;
using sga_back.Common;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using sga_back.Response;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class InscripcionesService : IInscripcionesService
{
    private readonly IInscripcionesRepository _repository;
    private readonly ICursosRepository _cursosRepository;
    private readonly IPagosRepository _pagosRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<InscripcionesService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public InscripcionesService(
         IInscripcionesRepository repository,
         ICursosRepository cursosRepository,
         IPagosRepository pagosRepository,
         IMapper mapper,
         ILogger<InscripcionesService> logger,
         IServiceProvider serviceProvider)
    {
        _repository = repository;
        _cursosRepository = cursosRepository;
        _pagosRepository = pagosRepository;
        _mapper = mapper;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<int> Insertar(InscripcionRequest request)
    {
        await ValidationHelper.ValidarAsync(request, _serviceProvider);

        CursoDetalleDto? curso = await _cursosRepository.ObtenerDetallePorId(request.IdCurso);
        if (curso == null)
            throw new ReglasdeNegocioException("El curso seleccionado no existe.");

        Inscripcion inscripcion = _mapper.Map<Inscripcion>(request);

        inscripcion.FechaInscripcion = request.FechaInscripcion ?? DateTime.UtcNow;
        inscripcion.Estado = string.IsNullOrWhiteSpace(inscripcion.Estado) ? "Activa" : inscripcion.Estado;
        inscripcion.MotivoDescuento ??= string.Empty;
        inscripcion.MotivoDescuentoPractica ??= string.Empty;
        inscripcion.MotivoDescuentoMatricula ??= string.Empty;

        var (pagoEncabezado, detalles) = ConstruirPlanPago(
            curso,
            request.MontoDescuento,
            request.MontoDescuentoPractica,
            request.MontoDescuentoMatricula);

        int idInscripcion = await _repository.InsertarConPagos(
            inscripcion,
            pagoEncabezado,
            detalles);

        _logger.LogInformation(
            "Inscripción creada correctamente con ID {IdInscripcion} para curso {Curso}",
            idInscripcion,
            curso.Nombre);

        return idInscripcion;
    }

    public async Task<InscripcionPlanPagoPreviewDto> ObtenerPreviewPlanPago(InscripcionRequest request)
    {
        await ValidationHelper.ValidarAsync(request, _serviceProvider);

        CursoDetalleDto? curso = await _cursosRepository.ObtenerDetallePorId(request.IdCurso);
        if (curso == null)
            throw new ReglasdeNegocioException("El curso seleccionado no existe.");

        var previewDetalles = new List<InscripcionPlanPagoDetalleDto>();

        var (pagoEncabezado, detalles) = ConstruirPlanPago(
            curso,
            request.MontoDescuento,
            request.MontoDescuentoPractica,
            request.MontoDescuentoMatricula,
            previewDetalles);

        return new InscripcionPlanPagoPreviewDto
        {
            IdCurso = curso.IdCurso,
            NombreCurso = curso.Nombre,
            Total = pagoEncabezado.Total,
            CantidadPagos = detalles.Count,
            DescuentoAplicado = pagoEncabezado.Descuento,
            Detalles = previewDetalles
        };
    }

    private (PagoEncabezado PagoEncabezado, List<PagoDetalle> Detalles) ConstruirPlanPago(
        CursoDetalleDto curso,
        decimal montoDescuento,
        decimal montoDescuentoPractica,
        decimal montoDescuentoMatricula,
        List<InscripcionPlanPagoDetalleDto>? previewDetalles = null)
    {
        _logger.LogInformation(
            "Construyendo plan de pagos para curso ID {IdCurso} - {Curso}",
            curso.IdCurso,
            curso.Nombre);

        if (curso.Conceptos == null || !curso.Conceptos.Any())
            throw new ReglasdeNegocioException("El curso no tiene conceptos configurados.");

        List<PagoDetalle> detalles = new();
        decimal descuentoTotalAplicado = 0;

        foreach (var concepto in curso.Conceptos.Where(x => x.Activo))
        {
            if (concepto.Vencimientos == null || !concepto.Vencimientos.Any())
                continue;

            foreach (var vencimiento in concepto.Vencimientos
                         .Where(v => v.Activo)
                         .OrderBy(v => v.NroOrden))
            {
                decimal montoOriginal = vencimiento.Monto;
                decimal descuentoAplicado = 0;

                if (string.Equals(concepto.TipoConcepto, "Matricula", StringComparison.OrdinalIgnoreCase))
                {
                    descuentoAplicado = montoDescuentoMatricula;
                }
                else if (string.Equals(concepto.TipoConcepto, "Practica", StringComparison.OrdinalIgnoreCase))
                {
                    descuentoAplicado = montoDescuentoPractica;
                }
                else if (string.Equals(concepto.TipoConcepto, "Cuota", StringComparison.OrdinalIgnoreCase))
                {
                    descuentoAplicado = montoDescuento;
                }

                descuentoAplicado = Math.Min(descuentoAplicado, montoOriginal);
                decimal montoFinal = montoOriginal - descuentoAplicado;

                if (montoFinal <= 0)
                    continue;

                descuentoTotalAplicado += descuentoAplicado;

                string descripcionConcepto = !string.IsNullOrWhiteSpace(vencimiento.Descripcion)
                    ? vencimiento.Descripcion!
                    : $"{concepto.TipoConcepto} {vencimiento.NroOrden}";

                detalles.Add(new PagoDetalle
                {
                    Concepto = $"{descripcionConcepto} - {curso.Nombre}",
                    Monto = montoFinal,
                    FechaVencimiento = vencimiento.FechaVencimiento,
                    Estado = "Pendiente"
                });

                if (previewDetalles != null)
                {
                    previewDetalles.Add(new InscripcionPlanPagoDetalleDto
                    {
                        TipoConcepto = concepto.TipoConcepto,
                        Concepto = descripcionConcepto,
                        MontoOriginal = montoOriginal,
                        DescuentoAplicado = descuentoAplicado,
                        MontoFinal = montoFinal,
                        FechaVencimiento = vencimiento.FechaVencimiento,
                        NroOrden = vencimiento.NroOrden
                    });
                }
            }
        }

        if (!detalles.Any())
            throw new ReglasdeNegocioException(
                "No se generaron pagos para la inscripción porque el curso no tiene vencimientos activos.");

        PagoEncabezado pagoEncabezado = new PagoEncabezado
        {
            Total = detalles.Sum(d => d.Monto),
            TipoCuenta = "Credito",
            Descuento = descuentoTotalAplicado,
            Observacion = $"Generación de pagos por inscripción - {curso.Nombre}"
        };

        return (pagoEncabezado, detalles);
    }

    public async Task<int> Actualizar(int idInscripcion, InscripcionRequest request)
    {
        await ValidationHelper.ValidarAsync(request, _serviceProvider);

        Inscripcion inscripcion = _mapper.Map<Inscripcion>(request);
        inscripcion.IdInscripcion = idInscripcion;
        inscripcion.Estado = string.IsNullOrWhiteSpace(inscripcion.Estado) ? "Activa" : inscripcion.Estado;
        inscripcion.MotivoDescuento ??= string.Empty;
        inscripcion.MotivoDescuentoPractica ??= string.Empty;
        inscripcion.MotivoDescuentoMatricula ??= string.Empty;

        return await _repository.Actualizar(inscripcion);
    }

    public async Task<bool> Eliminar(int idInscripcion)
    {
        return await _repository.Eliminar(idInscripcion);
    }

    public async Task<InscripcionResponse?> ObtenerPorId(int idInscripcion)
    {
        var inscripcion = await _repository.ObtenerPorId(idInscripcion);
        return inscripcion != null ? _mapper.Map<InscripcionResponse>(inscripcion) : null;
    }

    public async Task<IEnumerable<InscripcionDetalleDto>> ObtenerTodas(InscripcionFiltroRequest filtro)
    {
        return await _repository.ObtenerTodas(filtro);
    }

    public async Task<IEnumerable<EstudianteDto>> ObtenerEstudiantes(string? search)
    {
        return await _repository.ObtenerEstudiantes(search);
    }

    public async Task<IEnumerable<CursosInscripcionDto>> ObtenerCursos(string? search)
    {
        return await _repository.ObtenerCursos(search);
    }
}
