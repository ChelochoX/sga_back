using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using sga_back.Services.Interfaces;

namespace sga_back.Middlewares.Filters;

/// <summary>
/// Filtro que valida si el usuario autenticado tiene permisos para acceder a una acción específica de una entidad.
/// </summary>
public class PermisoRequeridoAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _entidad;
    private readonly string _recurso;

    /// <summary>
    /// Constructor del atributo.
    /// </summary>
    /// <param name="entidad">Nombre de la entidad (ej: "Inscripciones")</param>
    /// <param name="recurso">Nombre del recurso/acción (ej: "Crear")</param>
    public PermisoRequeridoAttribute(string entidad, string recurso)
    {
        _entidad = entidad;
        _recurso = recurso;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<PermisoRequeridoAttribute>>();
        var user = httpContext.User;

        try
        {
            // Verificar si el usuario está autenticado
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                logger.LogWarning("🔒 Usuario no autenticado.");
                context.Result = new UnauthorizedObjectResult(new
                {
                    Message = "No estás autenticado para acceder a este recurso."
                });
                return;
            }

            // Obtener ID del usuario desde el token
            var idUsuarioClaim = user.FindFirst("id_usuario")?.Value;
            if (string.IsNullOrEmpty(idUsuarioClaim) || !int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                logger.LogWarning("⚠️ Token sin ID de usuario válido.");
                context.Result = new UnauthorizedObjectResult(new
                {
                    Message = "No se pudo identificar al usuario en el token."
                });
                return;
            }

            // Validar si tiene el permiso correspondiente
            var permisoService = httpContext.RequestServices.GetRequiredService<IPermisosService>();
            var tienePermiso = await permisoService.TienePermiso(idUsuario, _entidad, _recurso);

            if (!tienePermiso)
            {
                logger.LogWarning("⛔ Permiso denegado: Usuario {IdUsuario} intentó '{Recurso}' sobre '{Entidad}' sin permiso.",
                    idUsuario, _recurso, _entidad);

                context.Result = new ObjectResult(new
                {
                    Message = $"No tenés permisos para realizar la acción '{_recurso}' sobre '{_entidad}'."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };

                return;
            }

            // Permiso concedido
            logger.LogInformation("✅ Permiso concedido: Usuario {IdUsuario} puede realizar '{Recurso}' sobre '{Entidad}'.",
                idUsuario, _recurso, _entidad);

            await next();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error al validar permiso para '{Recurso}' sobre '{Entidad}'.", _recurso, _entidad);

            context.Result = new ObjectResult(new
            {
                Message = "Ocurrió un error inesperado al verificar permisos.",
                Error = ex.Message // ⚠️ Podés ocultar este detalle en producción
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
