

using sga_back.Configurations;

string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configuración de Logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Puedes tener diferentes configuraciones según el entorno
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
}

// Add services to the container.
builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddControllers();
builder.Services.AddConfiguration(configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Documentacion API Sistema de Gestion Academica",
        Version = "v1",
        Description = "REST API de Sistema de Gestion Academica"
    });
    c.EnableAnnotations();


    string xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Aplicacion Iniciada Correctamente");

app.UseHttpsRedirection();
app.UseHandlingMiddleware();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
