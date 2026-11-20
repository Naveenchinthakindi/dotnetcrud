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
        private readonly ILogger<StudentController> _logger;

        // Inject the database connection helper and logger
        public StudentController(DbConnection dbConnection, ILogger<StudentController> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        // 1. GET ALL STUDENTS: GET /api/Student
        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            try
            {
                _logger.LogInformation("Fetching all students.");

                var students = new List<StudentResponse>();
                const string query = "SELECT * FROM students";

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

                _logger.LogInformation("Successfully retrieved {Count} students.", students.Count);
                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching students.");
                return StatusCode(500, new { message = "Error querying students database", error = ex.Message });
            }
        }

        // 2. GET STUDENT BY ID: GET /api/Student/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching student with ID: {Id}", id);

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

                    _logger.LogInformation("Found student with ID: {Id}", id);
                    return Ok(student);
                }

                _logger.LogWarning("Student with ID {Id} not found.", id);
                return NotFound($"Student with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching student with ID: {Id}", id);
                return StatusCode(500, new { message = "Error querying student", error = ex.Message });
            }
        }

        // 3. ADD STUDENT: POST /api/Student
        [HttpPost]
        public async Task<IActionResult> AddStudent([FromBody] AddStudentRequest request)
        {
            try
            {
                _logger.LogInformation("Adding new student: {Name}", request.Name);

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

                _logger.LogInformation("Successfully created student with ID: {Id}", newId);
                return CreatedAtAction(nameof(GetStudentById), new { id = newId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student.");
                return StatusCode(500, new { message = "Error saving student", error = ex.Message });
            }
        }

        // 4. UPDATE STUDENT: PUT /api/Student/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentRequest request)
        {
            try
            {
                _logger.LogInformation("Updating student with ID: {Id}", id);

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
                    _logger.LogWarning("Cannot update: Student with ID {Id} not found.", id);
                    return NotFound($"Student with ID {id} not found.");
                }

                var response = new StudentResponse
                {
                    Id = id,
                    Name = request.Name,
                    Email = request.Email,
                    Age = request.Age
                };

                _logger.LogInformation("Successfully updated student with ID: {Id}", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student with ID: {Id}", id);
                return StatusCode(500, new { message = "Error updating student", error = ex.Message });
            }
        }

        // 5. DELETE STUDENT: DELETE /api/Student/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                _logger.LogInformation("Deleting student with ID: {Id}", id);

                const string query = "DELETE FROM students WHERE id = @Id";

                await using var connection = _dbConnection.GetConnection();
                await connection.OpenAsync();

                await using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    _logger.LogWarning("Cannot delete: Student with ID {Id} not found.", id);
                    return NotFound($"Student with ID {id} not found.");
                }

                _logger.LogInformation("Successfully deleted student with ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student with ID: {Id}", id);
                return StatusCode(500, new { message = "Error deleting student", error = ex.Message });
            }
        }
    }
}
