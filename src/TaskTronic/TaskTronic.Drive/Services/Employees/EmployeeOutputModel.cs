namespace TaskTronic.Drive.Services.Employees
{
    using AutoMapper;
    using Data.Models;
    using TaskTronic.Models;

    public class EmployeeOutputModel : IMapFrom<Employee>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public void Mapping(Profile mapper)
            => mapper
                .CreateMap<Employee, EmployeeOutputModel>()
                .ForMember(e => e.Id, cfg => cfg
                    .MapFrom(e => e.EmployeeId));
    }
}
