using System.ComponentModel.DataAnnotations;

namespace Lab2_ImageService.Models
{
    public class UserModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
