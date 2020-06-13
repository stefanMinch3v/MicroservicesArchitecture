namespace TaskTronic.Identity.Services
{
    using Data.Models;
    using Microsoft.AspNetCore.Identity;
    using System.Linq;
    using System.Threading.Tasks;
    using TaskTronic.Services;
    using TaskTronic.Identity.Models;

    public class IdentityService : IIdentityService
    {
        private const string INVALID_CREDENTIALS = "Invalid credentials.";
        private const string EMPTY_FORM = "Empty form is not accepted.";

        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IJwtGeneratorService jwtGeneratorService;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtGeneratorService jwtGeneratorService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.jwtGeneratorService = jwtGeneratorService;
        }

        public async Task<Result<JwtModel>> LoginAsync(InputLoginModel model)
        {
            if (model is null)
            {
                return EMPTY_FORM;
            }

            var result = await this.signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (!result.Succeeded)
            {
                return INVALID_CREDENTIALS;
            }

            var user = await this.userManager.FindByNameAsync(model.Username);
            if (user is null)
            {
                return INVALID_CREDENTIALS;
            }

            return await this.jwtGeneratorService.GenerateTokenAsync(user);
        }

        public async Task<Result<bool>> RegisterAsync(InputRegisterModel model)
        {
            if (model is null)
            {
                return EMPTY_FORM;
            }

            var user = new ApplicationUser { Email = model.Email, UserName = model.Username };
            var result = await this.userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return Result<bool>.Failure(result.Errors.Select(e => e.Description));
            }

            return Result<bool>.SuccessWith(true);
        }
    }
}
