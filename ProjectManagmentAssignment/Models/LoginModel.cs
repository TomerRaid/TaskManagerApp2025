using System.ComponentModel.DataAnnotations;

namespace ProjectManagmentAppNew.Models
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
