using Microsoft.AspNetCore.Mvc;
using StudentApi.Models.Request;
using StudentApi.Services.Interfaces;

namespace StudentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // GET: api/Student
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(students);
        }

        // GET: api/Student/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);

            if (student == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            return Ok(student);
        }

        // POST: api/Student
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddStudentRequest request)
        {
            var student = await _studentService.AddStudentAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = student.Id },
                student
            );
        }

        // PUT: api/Student/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request)
        {
            var updatedStudent = await _studentService.UpdateStudentAsync(id, request);

            if (updatedStudent == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            return Ok(updatedStudent);
        }

        // DELETE: api/Student/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _studentService.DeleteStudentAsync(id);

            if (!deleted)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            return NoContent();
        }
    }
}
