namespace TaskTronic.Admin.Models.Employees
{
    using System.ComponentModel.DataAnnotations;
    using TaskTronic.Models;

    public class EmployeeFormModel : IMapFrom<EmployeeDetailsOutputModel>
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
