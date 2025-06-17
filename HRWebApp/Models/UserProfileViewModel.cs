using System.ComponentModel.DataAnnotations;

namespace HRWebApp.Models
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }

        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        public bool HasEmployeeProfile { get; set; }

        public int? EmployeeId { get; set; }

        public bool IsAdmin { get; set; }

        [Display(Name = "Department")]
        public string? DepartmentName { get; set; }

        public decimal? HourlyRate { get; set; }
    }
}