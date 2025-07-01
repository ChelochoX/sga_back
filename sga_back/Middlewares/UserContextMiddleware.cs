using sga_back.Common;
using sga_back.Repositories.Interfaces;

namespace sga_back.Middlewares;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, UserContext userContext, IUsuariosRepository usuarioRepo)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var idUsuarioStr = user.FindFirst("id_usuario")?.Value;

            if (int.TryParse(idUsuarioStr, out var idUsuario))
            {
                var usuarioDb = await usuarioRepo.ObtenerUsuarioActivoPorId(idUsuario);
                if (usuarioDb != null)
                {
                    userContext.IdUsuario = usuarioDb.IdUsuario;
                    userContext.NombreUsuario = usuarioDb.NombreUsuario;
                    // Puedes agregar más datos si querés
                }
            }
        }

        await _next(context);
    }

}
