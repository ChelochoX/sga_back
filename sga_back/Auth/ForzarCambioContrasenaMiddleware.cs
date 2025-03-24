namespace sga_back.Auth;

public class ForzarCambioContrasenaMiddleware
{
    private readonly RequestDelegate _next;

    public ForzarCambioContrasenaMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requiereCambio = context.User?.FindFirst("requiere_cambio_contrasena")?.Value;

        if (requiereCambio == "True" && context.Request.Path != "/api/Auth/cambiar-contrasena")
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Debes cambiar tu contraseña antes de continuar.");
            return;
        }

        await _next(context);
    }
}
