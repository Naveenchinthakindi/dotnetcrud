using StudentApi.Models.Request;
using StudentApi.Models.Response;

namespace StudentApi.Services.Interfaces
{
    public interface IStudentService
    {
        Task<List<StudentResponse>> GetAllStudentsAsync();
        Task<StudentResponse?> GetStudentByIdAsync(int id);
        Task<StudentResponse> AddStudentAsync(AddStudentRequest request);
        Task<StudentResponse?> UpdateStudentAsync(int id, UpdateStudentRequest request);
        Task<bool> DeleteStudentAsync(int id);
    }
}
