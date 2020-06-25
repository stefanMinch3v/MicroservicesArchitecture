namespace TaskTronic.Admin.Models.Employees
{
    using TaskTronic.Models;

    public class EmployeeInputModel : IMapFrom<EmployeeFormModel>
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }
}
