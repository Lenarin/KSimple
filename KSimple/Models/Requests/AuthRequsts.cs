using System.ComponentModel.DataAnnotations;

namespace KSimple.Models.Requests
{
    public class LoginRequest
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}