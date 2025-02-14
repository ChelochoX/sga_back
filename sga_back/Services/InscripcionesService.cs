﻿using AutoMapper;
using sga_back.Common;
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

        // Obtener información del curso
        Curso? curso = await _cursosRepository.ObtenerPorId(request.IdCurso);
        if (curso == null)
        {
            throw new ReglasdeNegocioException("El curso seleccionado no existe.");
        }

        // Generar pagos asociados a la inscripción considerando los descuentos
        await GenerarPagosPorInscripcion(idInscripcion, curso, request.MontoDescuento, request.MontoDescuentoPractica, inscripcion.FechaInscripcion);

        return idInscripcion;
    }

    private async Task GenerarPagosPorInscripcion(int idInscripcion, Curso curso, decimal montoDescuento, decimal montoDescuentoPractica, DateTime fechaInscripcion)
    {
        _logger.LogInformation("Generando pagos para la inscripción ID: {IdInscripcion}, Curso: {CursoNombre}", idInscripcion, curso.Nombre);

        List<PagoDetalle> detalles = new List<PagoDetalle>();
        DateTime fechaVencimiento = fechaInscripcion; // Primera fecha de vencimiento es la de la inscripción

        // Aplicar descuentos antes de calcular las cuotas
        decimal totalCurso = curso.MontoCuota * curso.CantidadCuota - montoDescuento;
        decimal totalPractica = (curso.TienePractica == 'S') ? curso.CostoPractica * curso.CantidadCuota - montoDescuentoPractica : 0;

        // Asegurar que los valores no sean negativos
        totalCurso = Math.Max(totalCurso, 0);
        totalPractica = Math.Max(totalPractica, 0);

        // 🔹 1. Agregar el pago de matrícula como primer concepto 🔹
        detalles.Add(new PagoDetalle
        {
            Concepto = $"Matrícula - {curso.Nombre}",
            Monto = curso.MontoMatricula,
            FechaVencimiento = fechaInscripcion, // La matrícula vence el mismo día de la inscripción
            Estado = "Pendiente"
        });

        // 🔹 2. Generar las cuotas del curso y las cuotas de práctica en paralelo 🔹
        decimal montoPorCuota = totalCurso / curso.CantidadCuota;
        decimal montoPorPractica = (curso.TienePractica == 'S') ? totalPractica / curso.CantidadCuota : 0;

        for (int i = 1; i <= curso.CantidadCuota; i++)
        {
            // Cuota del curso
            detalles.Add(new PagoDetalle
            {
                Concepto = $"Cuota {i} - {curso.Nombre}",
                Monto = montoPorCuota,
                FechaVencimiento = fechaVencimiento,
                Estado = "Pendiente"
            });

            // Cuota de práctica (se genera junto con la cuota del curso)
            if (curso.TienePractica == 'S' && montoPorPractica > 0)
            {
                detalles.Add(new PagoDetalle
                {
                    Concepto = $"Práctica {i} - {curso.Nombre}",
                    Monto = montoPorPractica,
                    FechaVencimiento = fechaVencimiento,
                    Estado = "Pendiente"
                });
            }

            fechaVencimiento = fechaVencimiento.AddMonths(1); // Se avanza un mes para la siguiente cuota
        }

        // 🔹 3. Insertar pago en la base de datos 🔹
        PagoEncabezado pagoEncabezado = new PagoEncabezado
        {
            IdInscripcion = idInscripcion,
            Total = detalles.Sum(d => d.Monto),
            TipoCuenta = "Credito", // Se asume que es crédito hasta que se pague
            Descuento = montoDescuento + montoDescuentoPractica,
            Observacion = $"Generación de pagos por inscripción - {curso.Nombre}"
        };

        await _pagosRepository.InsertarPagoConDetalles(pagoEncabezado, detalles);
        _logger.LogInformation("Pagos generados exitosamente para la inscripción ID: {IdInscripcion}", idInscripcion);
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

    public async Task<IEnumerable<InscripcionResponse>> ObtenerTodas()
    {
        var inscripciones = await _repository.ObtenerTodas();
        return _mapper.Map<IEnumerable<InscripcionResponse>>(inscripciones);
    }

}
