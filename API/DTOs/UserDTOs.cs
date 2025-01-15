using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UserRequest
    {
        [Required(ErrorMessage = "empty name", AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(ErrorMessage = "empty email", AllowEmptyStrings = false)]
        public string Email { get; set; }

        [Required(ErrorMessage = "empty password", AllowEmptyStrings = false)]
        [MinLength(8, ErrorMessage = "password must have at least 8 characters")]
        public string Password { get; set; }

        public bool IsAdmin { get; set; }
    }

    public class SignInRequest
    {
        [Required(ErrorMessage = "empty email", AllowEmptyStrings = false)]
        public string Email { get; set; }

        [Required(ErrorMessage = "empty password", AllowEmptyStrings = false)]
        [MinLength(8, ErrorMessage = "password must have at least 8 characters")]
        public string Password { get; set; }
    }
}
