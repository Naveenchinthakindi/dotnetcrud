using System.ComponentModel.DataAnnotations;

namespace StudentApi.Models.Request
{
    public class AddStudentRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120")]
        public int Age { get; set; }
    }
}
