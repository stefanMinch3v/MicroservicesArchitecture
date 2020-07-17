namespace TaskTronic.Identity.Services.Jwt
{
    using Data.Models;
    using Models;
    using System.Threading.Tasks;

    public interface IJwtGeneratorService
    {
        Task<JwtOutputModel> GenerateTokenAsync(ApplicationUser appUser);
    }
}
