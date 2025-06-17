using System.ComponentModel.DataAnnotations;
using HRWebApp.Enums;

namespace HRWebApp.Models.Authentication
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First Name can't be blank ")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name can't be blank ")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Email can't be blank ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password can't be blank ")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password can't be blank ")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
