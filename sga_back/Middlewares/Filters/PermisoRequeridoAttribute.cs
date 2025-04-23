using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using sga_back.Services.Interfaces;

namespace sga_back.Middlewares.Filters;

public class PermisoRequeridoAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _entidad;
    private readonly string _recurso;

    public PermisoRequeridoAttribute(string recurso, string entidad)
    {
        _recurso = recurso;
        _entidad = entidad;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var idUsuarioClaim = user.FindFirst("id_usuario")?.Value;
        if (string.IsNullOrEmpty(idUsuarioClaim) || !int.TryParse(idUsuarioClaim, out int idUsuario))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permisoService = context.HttpContext.RequestServices.GetRequiredService<IPermisosService>();
        var tienePermiso = await permisoService.TienePermiso(idUsuario, _entidad, _recurso);

        if (!tienePermiso)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
