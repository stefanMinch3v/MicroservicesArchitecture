namespace TaskTronic.Identity.Services
{
    using Models;
    using System.Threading.Tasks;
    using TaskTronic.Services;

    public interface IIdentityService
    {
        Task<Result<bool>> RegisterAsync(InputRegisterModel model);

        Task<Result<JwtModel>> LoginAsync(InputLoginModel model);
    }
}
