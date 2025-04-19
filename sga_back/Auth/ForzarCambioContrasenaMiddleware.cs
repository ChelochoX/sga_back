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
        var path = context.Request.Path.Value.ToLower();

        // 🔹 Permitir que todos accedan a cambiar contraseña
        if (path == "/api/auth/cambiar-contrasena")
        {
            await _next(context);
            return;
        }

        // 🔹 Si requiere cambiar la contraseña, bloquear otros endpoints
        if (requiereCambio == "True")
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Debes cambiar tu contraseña antes de continuar.");
            return;
        }

        await _next(context);
    }
}
