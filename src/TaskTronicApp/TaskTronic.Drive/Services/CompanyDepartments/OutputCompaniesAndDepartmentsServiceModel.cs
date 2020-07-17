namespace TaskTronic.Drive.Services.CompanyDepartments
{
    using System.Collections.Generic;

    public class OutputCompaniesAndDepartmentsServiceModel
    {
        public IReadOnlyList<OutputCompanyDepartmentsServiceModel> Companies { get; set; }

        public IReadOnlyList<OutputDepartmentServiceModel> Departments { get; set; }
    }
}
