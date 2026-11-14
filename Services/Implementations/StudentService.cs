using StudentApi.DAL.Interfaces;
using StudentApi.Models.Entities;
using StudentApi.Models.Request;
using StudentApi.Models.Response;
using StudentApi.Services.Interfaces;

namespace StudentApi.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IStudentDataAccess _studentDataAccess;

        public StudentService(IStudentDataAccess studentDataAccess)
        {
            _studentDataAccess = studentDataAccess;
        }

        public async Task<List<StudentResponse>> GetAllStudentsAsync()
        {
            var students = await _studentDataAccess.GetAllStudentsAsync();

            return students.Select(std => new StudentResponse
            {
                Id = std.Id,
                Name = std.Name,
                Email = std.Email,
                Age = std.Age
            }).ToList();
        }

        public async Task<StudentResponse?> GetStudentByIdAsync(int id)
        {
            var student = await _studentDataAccess.GetStudentByIdAsync(id);

            if (student == null)
            {
                return null;
            }

            return new StudentResponse
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Age = student.Age
            };
        }

        public async Task<StudentResponse> AddStudentAsync(AddStudentRequest request)
        {
            var student = new Student
            {
                Name = request.Name,
                Email = request.Email,
                Age = request.Age
            };

            int studentId = await _studentDataAccess.AddStudentAsync(student);

            return new StudentResponse
            {
                Id = studentId,
                Name = student.Name,
                Email = student.Email,
                Age = student.Age
            };
        }

        public async Task<StudentResponse?> UpdateStudentAsync(int id, UpdateStudentRequest request)
        {
            var existingStudent = await _studentDataAccess.GetStudentByIdAsync(id);

            if (existingStudent == null)
            {
                return null;
            }

            var studentToUpdate = new Student
            {
                Id = id,
                Name = request.Name,
                Email = request.Email,
                Age = request.Age
            };

            await _studentDataAccess.UpdateStudentAsync(studentToUpdate);

            return new StudentResponse
            {
                Id = id,
                Name = studentToUpdate.Name,
                Email = studentToUpdate.Email,
                Age = studentToUpdate.Age
            };
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            return await _studentDataAccess.DeleteStudentAsync(id);
        }
    }
}
