using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly DbConnection _dbConnection;

        // Inject the single database connection helper
        public StudentController(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // 1. GET ALL STUDENTS: GET /api/Student
        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = new List<StudentResponse>();
            const string query = "SELECT id, name, email, age FROM students";

            await using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                students.Add(new StudentResponse
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Name = reader["name"].ToString() ?? string.Empty,
                    Email = reader["email"].ToString() ?? string.Empty,
                    Age = Convert.ToInt32(reader["age"])
                });
            }

            return Ok(students);
        }

        // 2. GET STUDENT BY ID: GET /api/Student/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            const string query = "SELECT id, name, email, age FROM students WHERE id = @Id";

            await using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var student = new StudentResponse
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Name = reader["name"].ToString() ?? string.Empty,
                    Email = reader["email"].ToString() ?? string.Empty,
                    Age = Convert.ToInt32(reader["age"])
                };

                return Ok(student);
            }

            return NotFound($"Student with ID {id} not found.");
        }

        // 3. ADD STUDENT: POST /api/Student
        [HttpPost]
        public async Task<IActionResult> AddStudent([FromBody] AddStudentRequest request)
        {
            const string query = "INSERT INTO students (name, email, age) VALUES (@Name, @Email, @Age);";

            await using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", request.Name);
            command.Parameters.AddWithValue("@Email", request.Email);
            command.Parameters.AddWithValue("@Age", request.Age);

            await command.ExecuteNonQueryAsync();

            int newId = (int)command.LastInsertedId;

            var response = new StudentResponse
            {
                Id = newId,
                Name = request.Name,
                Email = request.Email,
                Age = request.Age
            };

            return CreatedAtAction(nameof(GetStudentById), new { id = newId }, response);
        }

        // 4. UPDATE STUDENT: PUT /api/Student/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentRequest request)
        {
            const string query = "UPDATE students SET name = @Name, email = @Email, age = @Age WHERE id = @Id";

            await using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", request.Name);
            command.Parameters.AddWithValue("@Email", request.Email);
            command.Parameters.AddWithValue("@Age", request.Age);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            var response = new StudentResponse
            {
                Id = id,
                Name = request.Name,
                Email = request.Email,
                Age = request.Age
            };

            return Ok(response);
        }

        // 5. DELETE STUDENT: DELETE /api/Student/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            const string query = "DELETE FROM students WHERE id = @Id";

            await using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            return NoContent();
        }
    }
}
