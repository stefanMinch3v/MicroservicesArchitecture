﻿namespace TaskTronic.Drive.Services.CompanyDepartments
{
    using AutoMapper;
    using System.Collections.Generic;
    using TaskTronic.Drive.Data.Models;
    using TaskTronic.Models;

    public class OutputCompanyDepartmentsServiceModel : IMapFrom<Company>
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }

        public IReadOnlyList<OutputDepartmentServiceModel> Departments { get; set; }

        public void Mapping(Profile mapper)
            => mapper
                .CreateMap<Company, OutputCompanyDepartmentsServiceModel>();
    }
}