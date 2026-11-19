using MySql.Data.MySqlClient;

namespace StudentApi.Data
{
    // Single helper class to manage MySQL database connection
    public class DbConnection
    {
        private readonly string _connectionString;
        private readonly ILogger<DbConnection> _logger;

        public DbConnection(IConfiguration configuration, ILogger<DbConnection> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
            _logger = logger;
        }

        // Returns a new MySQL connection
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        //check connection
       public async Task CheckConnection(){
        try{
            using var connection = GetConnection();
            await connection.OpenAsync();

            _logger.LogInformation("MySQL Connected Succesffully");
            
        }catch(Exception ex){
            _logger.LogError(ex, "MySQL Connection is Failed");
        }
       }
    }
}
