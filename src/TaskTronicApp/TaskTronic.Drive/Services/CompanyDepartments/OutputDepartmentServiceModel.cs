namespace TaskTronic.Drive.Services.CompanyDepartments
{
    using TaskTronic.Models;
    using Data.Models;
    using AutoMapper;

    public class OutputDepartmentServiceModel : IMapFrom<CompanyDepartments>
    {
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; }

        public void Mapping(Profile mapper)
            => mapper
                .CreateMap<CompanyDepartments, OutputDepartmentServiceModel>()
                .ForMember(d => d.DepartmentName, cfg => cfg.MapFrom(src => src.Department.Name));
    }
}
