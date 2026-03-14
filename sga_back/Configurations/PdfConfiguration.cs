using sga_back.DTOs;

namespace sga_back.Configurations;

public static class PdfConfiguration
{
    public static void AddPdfGeneration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FacturaPdfSettings>(configuration.GetSection("FacturaPdf"));
    }
}
