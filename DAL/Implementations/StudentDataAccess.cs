using MySql.Data.MySqlClient;
using StudentApi.DAL.Interfaces;
using StudentApi.Models.Entities;

namespace StudentApi.DAL.Implementations
{
    public class StudentDataAccess : IStudentDataAccess
    {
        private readonly MySqlConnectionFactory _connectionFactory;

        public StudentDataAccess(MySqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            var students = new List<Student>();
            const string query = "SELECT id, name, email, age FROM students";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                students.Add(new Student
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Age = reader.GetInt32(reader.GetOrdinal("age"))
                });
            }

            return students;
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            const string query = "SELECT id, name, email, age FROM students WHERE id = @Id";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Student
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Age = reader.GetInt32(reader.GetOrdinal("age"))
                };
            }

            return null;
        }

        public async Task<int> AddStudentAsync(Student student)
        {
            const string query = "INSERT INTO students (name, email, age) VALUES (@Name, @Email, @Age)";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", student.Name);
            command.Parameters.AddWithValue("@Email", student.Email);
            command.Parameters.AddWithValue("@Age", student.Age);

            await command.ExecuteNonQueryAsync();
            return (int)command.LastInsertedId;
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            const string query = "UPDATE students SET name = @Name, email = @Email, age = @Age WHERE id = @Id";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", student.Id);
            command.Parameters.AddWithValue("@Name", student.Name);
            command.Parameters.AddWithValue("@Email", student.Email);
            command.Parameters.AddWithValue("@Age", student.Age);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            const string query = "DELETE FROM students WHERE id = @Id";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}
