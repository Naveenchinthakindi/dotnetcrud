using Microsoft.Data.SqlClient;

namespace StudentApi.Data
{
    public class SapDbConnection
    {
        private readonly string _connectionString;
        private readonly ILogger<SapDbConnection> _logger;

        public SapDbConnection(IConfiguration configuration, ILogger<SapDbConnection> logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("SapConnection")
                ?? configuration["connectApp:ConnString"]
                ?? throw new InvalidOperationException("SAP SQL Connection string ('SapConnection' or 'connectApp:ConnString') not found in appsettings.json.");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                await using var connection = GetConnection();
                await connection.OpenAsync();
                _logger.LogInformation("Successfully connected to SAP B1 database.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to SAP B1 database: {Message}", ex.Message);
                return false;
            }
        }
    }
}
