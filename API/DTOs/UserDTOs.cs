using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class UserRequest
    {

        [Required(ErrorMessage = "empty name", AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(ErrorMessage = "empty email", AllowEmptyStrings = false)]
        [RegularExpression(".+\\@.+\\..+", ErrorMessage = "invalid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "empty password", AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}
