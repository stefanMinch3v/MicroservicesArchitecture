namespace TaskTronic.Drive.Services.CompanyDepartments
{
    using Data.Models;
    using TaskTronic.Models;

    public class OutputDepartmentServiceModel : IMapFrom<Department>
    {
        public int DepartmentId { get; set; }

        public string Name { get; set; }
    }
}
