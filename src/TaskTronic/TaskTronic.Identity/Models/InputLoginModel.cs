namespace TaskTronic.Identity.Models
{
    using System.ComponentModel.DataAnnotations;

    public class InputLoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
