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

        Inscripcion inscripcion = _mapper.Map<Inscripcion>(request);
        var idInscripcion = await _repository.Insertar(inscripcion);

        inscripcion.FechaInscripcion = request.FechaInscripcion ?? DateTime.UtcNow;

        // Ahora obtenemos el curso con el nuevo modelo
        CursoDetalleDto? curso = await _cursosRepository.ObtenerDetallePorId(request.IdCurso);
        if (curso == null)
        {
            throw new ReglasdeNegocioException("El curso seleccionado no existe.");
        }

        // Generar pagos asociados a la inscripción considerando la nueva estructura
        await GenerarPagosPorInscripcion(
            idInscripcion,
            curso,
            request.MontoDescuento,
            request.MontoDescuentoPractica,
            request.MontoDescuentoMatricula,
            inscripcion.FechaInscripcion);

        return idInscripcion;
    }

    private async Task GenerarPagosPorInscripcion(
        int idInscripcion,
        CursoDetalleDto curso,
        decimal montoDescuento,
        decimal montoDescuentoPractica,
        decimal montoDescuentoMatricula,
        DateTime fechaInscripcion)
    {
        _logger.LogInformation(
            "Generando pagos para la inscripción ID: {IdInscripcion}, Curso: {CursoNombre}",
            idInscripcion,
            curso.Nombre);

        List<PagoDetalle> detalles = new();

        if (curso.Conceptos == null || !curso.Conceptos.Any())
        {
            throw new ReglasdeNegocioException("El curso no tiene conceptos configurados.");
        }

        foreach (var concepto in curso.Conceptos.Where(x => x.Activo))
        {
            if (concepto.Vencimientos == null || !concepto.Vencimientos.Any())
                continue;

            foreach (var vencimiento in concepto.Vencimientos
                         .Where(v => v.Activo)
                         .OrderBy(v => v.NroOrden))
            {
                decimal monto = vencimiento.Monto;

                // Aplicar descuentos según el tipo de concepto
                if (string.Equals(concepto.TipoConcepto, "Matricula", StringComparison.OrdinalIgnoreCase))
                {
                    monto -= montoDescuentoMatricula;
                }
                else if (string.Equals(concepto.TipoConcepto, "Practica", StringComparison.OrdinalIgnoreCase))
                {
                    monto -= montoDescuentoPractica;
                }
                else if (string.Equals(concepto.TipoConcepto, "Cuota", StringComparison.OrdinalIgnoreCase))
                {
                    monto -= montoDescuento;
                }

                monto = Math.Max(monto, 0);

                if (monto == 0)
                    continue;

                string descripcionConcepto = !string.IsNullOrWhiteSpace(vencimiento.Descripcion)
                    ? vencimiento.Descripcion!
                    : $"{concepto.TipoConcepto} {vencimiento.NroOrden}";

                detalles.Add(new PagoDetalle
                {
                    Concepto = $"{descripcionConcepto} - {curso.Nombre}",
                    Monto = monto,
                    FechaVencimiento = vencimiento.FechaVencimiento,
                    Estado = "Pendiente"
                });
            }
        }

        if (!detalles.Any())
        {
            throw new ReglasdeNegocioException("No se generaron pagos para la inscripción porque el curso no tiene vencimientos activos.");
        }

        PagoEncabezado pagoEncabezado = new PagoEncabezado
        {
            IdInscripcion = idInscripcion,
            Total = detalles.Sum(d => d.Monto),
            TipoCuenta = "Credito",
            Descuento = montoDescuento + montoDescuentoPractica + montoDescuentoMatricula,
            Observacion = $"Generación de pagos por inscripción - {curso.Nombre}"
        };

        await _pagosRepository.InsertarPagoConDetalles(pagoEncabezado, detalles);

        _logger.LogInformation(
            "Pagos generados exitosamente para la inscripción ID: {IdInscripcion}",
            idInscripcion);
    }

    public async Task<int> Actualizar(int idInscripcion, InscripcionRequest request)
    {
        Inscripcion inscripcion = _mapper.Map<Inscripcion>(request);
        inscripcion.IdInscripcion = idInscripcion;

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
