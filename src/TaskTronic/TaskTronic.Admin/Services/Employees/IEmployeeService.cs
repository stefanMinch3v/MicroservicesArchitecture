namespace TaskTronic.Admin.Services.Employees
{
    using Models.Employees;
    using Refit;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEmployeeService
    {
        [Get("/Employees")]
        Task<IReadOnlyCollection<EmployeeDetailsOutputModel>> All();

        [Get("/Employees/{id}")]
        Task<EmployeeDetailsOutputModel> Details(int id);

        [Put("/Employees/{id}")]
        Task Edit(int id, EmployeeInputModel employee);
    }
}
