namespace TaskTronic.Web.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Web.Models;

    public class TaskTronicDbContext : IdentityDbContext<ApplicationUser>
    {
        public TaskTronicDbContext(DbContextOptions<TaskTronicDbContext> options)
            : base(options)
        {
        }
    }
}
