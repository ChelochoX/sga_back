﻿using Microsoft.AspNetCore.Mvc;
using sga_back.Request;
using sga_back.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;


namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagosController : ControllerBase
{
    private readonly IPagosService _service;
    private readonly ILogger<PagosController> _logger;

    public PagosController(ILogger<PagosController> logger, IPagosService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost("RegistrarPago")]
    [SwaggerOperation(
        Summary = "Registra un nuevo pago",
        Description = "Este endpoint permite registrar un nuevo pago junto con sus detalles asociados.")]
    public async Task<IActionResult> InsertarPago([FromBody] PagoRequest request)
    {
        _logger.LogInformation("Solicitud para registrar un nuevo pago.");
        int id = await _service.InsertarPagoConDetalles(request);
        _logger.LogInformation("Pago registrado con éxito. ID: {IdPago}", id);
        return Ok(id);
    }


    [HttpPut("ActualizarPago/{id}")]
    [SwaggerOperation(
        Summary = "Actualiza un pago existente",
        Description = "Este endpoint permite actualizar un pago junto con sus detalles por su ID.")]
    public async Task<IActionResult> ActualizarPago(int id, [FromBody] PagoRequest request)
    {
        _logger.LogInformation("Solicitud para actualizar el pago con ID: {IdPago}", id);
        await _service.ActualizarPagoConDetalles(id, request);
        _logger.LogInformation("Pago con ID {IdPago} actualizado con éxito.", id);
        return NoContent();
    }


    [HttpGet("ObtenerPago/{id}")]
    [SwaggerOperation(
        Summary = "Obtiene un pago por ID",
        Description = "Este endpoint permite obtener la información de un pago específico por su ID.")]
    public async Task<IActionResult> ObtenerPagoPorId(int id)
    {
        _logger.LogInformation("Solicitud para obtener el pago con ID: {IdPago}", id);
        var pago = await _service.ObtenerPagoPorId(id);
        return Ok(pago);
    }


    [HttpDelete("EliminarPago/{id}")]
    [SwaggerOperation(
        Summary = "Elimina un pago por ID",
        Description = "Este endpoint permite eliminar un pago y sus detalles asociados por su ID.")]
    public async Task<IActionResult> EliminarPago(int id)
    {
        _logger.LogInformation("Solicitud para eliminar el pago con ID: {IdPago}", id);
        await _service.EliminarPago(id);
        _logger.LogInformation("Pago con ID {IdPago} eliminado con éxito.", id);
        return NoContent();
    }

}
