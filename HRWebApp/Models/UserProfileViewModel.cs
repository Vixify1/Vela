using System.ComponentModel.DataAnnotations;

namespace HRWebApp.Models
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Address")]
        public string Address { get; set; }

        public bool HasEmployeeProfile { get; set; }

        public int? EmployeeId { get; set; }

        public bool IsAdmin { get; set; }
    }
}