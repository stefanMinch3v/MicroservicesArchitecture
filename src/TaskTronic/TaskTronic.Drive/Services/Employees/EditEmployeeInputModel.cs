namespace TaskTronic.Drive.Services.Employees
{
    using System.ComponentModel.DataAnnotations;

    // TODO: validations
    public class EditEmployeeInputModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
