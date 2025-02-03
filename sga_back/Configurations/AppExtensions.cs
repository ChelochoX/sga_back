using sga_back.Middlewares;

namespace sga_back.Configurations;

public static class AppExtensions
{
    public static void UseHandlingMiddleware(this IApplicationBuilder app)
    {
        _ = app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
