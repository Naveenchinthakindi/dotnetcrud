using StudentApi.Models.Entities;

namespace StudentApi.DAL.Interfaces
{
    public interface IStudentDataAccess
    {
        Task<List<Student>> GetAllStudentsAsync();
        Task<Student?> GetStudentByIdAsync(int id);
        Task<int> AddStudentAsync(Student student);
        Task<bool> UpdateStudentAsync(Student student);
        Task<bool> DeleteStudentAsync(int id);
    }
}
