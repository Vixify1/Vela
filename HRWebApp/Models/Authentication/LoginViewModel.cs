using System.ComponentModel.DataAnnotations;

namespace HRWebApp.Models.Authentication
{

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email can't be blank")]
        [EmailAddress(ErrorMessage = "Email should be in a proper email address format ")]
        [DataType(DataType.Password)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password can't be blank")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}


