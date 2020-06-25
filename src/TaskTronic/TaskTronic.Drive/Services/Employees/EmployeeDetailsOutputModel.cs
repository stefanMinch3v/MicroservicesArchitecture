namespace TaskTronic.Drive.Services.Employees
{
    using AutoMapper;
    using Data.Models;

    public class EmployeeDetailsOutputModel : EmployeeOutputModel
    {
        public int TotalFolders { get; set; }

        public int TotalFiles { get; set; }

        public void Mapping(Profile mapper)
            => mapper
                .CreateMap<Employee, EmployeeDetailsOutputModel>()
                .IncludeBase<Employee, EmployeeOutputModel>();
    }
}
