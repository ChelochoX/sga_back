﻿using Microsoft.AspNetCore.Mvc;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CursosController : ControllerBase
{
    private readonly ICursosService _service;
    private readonly ILogger<CursosController> _logger;

    public CursosController(ILogger<CursosController> logger, ICursosService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] CursoRequest request)
    {
        int id = await _service.Insertar(request);
        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CursoRequest request)
    {
        await _service.Actualizar(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.Eliminar(id);
        return NoContent();
    }

    [HttpPost("obtener-cursos")]
    public async Task<IActionResult> ObtenerCursos([FromBody] ObtenerCursosRequest request)
    {
        var cursos = await _service.ObtenerCursosPorFecha(request);
        return Ok(cursos);
    }

    [HttpPut("{id}/cambiar-estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoCursoRequest request)
    {
        await _service.CambiarEstado(id, request.Activo);
        return NoContent();

    }

}
