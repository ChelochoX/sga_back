using Microsoft.AspNetCore.Mvc;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonasController : ControllerBase
{
    private readonly IPersonasService _service;
    private readonly ILogger<PersonasController> _logger;

    public PersonasController(ILogger<PersonasController> logger, IPersonasService service)
    {
        _logger = logger;
        _service = service;
    }



}
