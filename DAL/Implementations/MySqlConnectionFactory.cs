using MySql.Data.MySqlClient;

namespace StudentApi.DAL.Implementations
{
    public class MySqlConnectionFactory
    {
        private readonly string _connectionString;

        public MySqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("MySQL Connection string is missing");
        }

        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
