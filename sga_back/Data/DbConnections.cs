using Microsoft.Data.SqlClient;
using sga_back.Exceptions;
using System.Data;

namespace sga_back.Data;

public class DbConnections
{
    private readonly IConfiguration _configuration;

    public DbConnections(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateSqlConnection()
    {
        string sqlConnectionString = BuildConnectionString(_configuration, "ConexionDB");
        return new SqlConnection(sqlConnectionString);
    }

    private string BuildConnectionString(IConfiguration configuration, string name)
    {
        IConfigurationSection connectionSettings = configuration.GetSection($"ConnectionStrings:{name}");

        if (connectionSettings == null || !connectionSettings.Exists())
        {
            throw new ParametroFaltanteCadenaConexionException($"Detalles de conexión para '{name}' no encontrados.");
        }

        string? server = connectionSettings["Server"];
        string? initialCatalog = connectionSettings["InitialCatalog"];
        string? userId = connectionSettings["UserId"];
        string? password = connectionSettings["Pwd"];
        bool? multipleActiveResultSets = connectionSettings.GetValue<bool?>("MultipleActiveResultSets");
        bool? pooling = connectionSettings.GetValue<bool?>("Pooling");
        int? maxPoolSize = connectionSettings.GetValue<int?>("MaxPoolSize");
        int? minPoolSize = connectionSettings.GetValue<int?>("MinPoolSize");
        bool? encrypt = connectionSettings.GetValue<bool?>("Encrypt");
        bool? trustServerCertificate = connectionSettings.GetValue<bool?>("TrustServerCertificate");

        if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(initialCatalog))
        {
            throw new ParametroFaltanteCadenaConexionException("Uno o más parámetros requeridos de la cadena de conexión son nulos o están vacíos.");
        }

        string db = initialCatalog;

        SqlConnectionStringBuilder builder = new()
        {
            DataSource = server,
            InitialCatalog = db,
            UserID = userId,
            Password = password
        };

        if (multipleActiveResultSets.HasValue)
        {
            builder.MultipleActiveResultSets = multipleActiveResultSets.Value;
        }

        if (pooling.HasValue)
        {
            builder.Pooling = pooling.Value;
        }

        if (maxPoolSize.HasValue)
        {
            builder.MaxPoolSize = maxPoolSize.Value;
        }

        if (minPoolSize.HasValue)
        {
            builder.MinPoolSize = minPoolSize.Value;
        }

        if (encrypt.HasValue)
        {
            builder.Encrypt = encrypt.Value;
        }

        if (trustServerCertificate.HasValue)
        {
            builder.TrustServerCertificate = trustServerCertificate.Value;
        }

        return builder.ConnectionString;
    }
}
