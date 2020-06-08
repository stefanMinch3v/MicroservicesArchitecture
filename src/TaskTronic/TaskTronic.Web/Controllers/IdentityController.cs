namespace TaskTronic.Web.Controllers
{
    using Extensions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using ViewModels;

    public class IdentityController : BaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<IdentityController> logger;
        private readonly ApplicationSettings applicationSettings;

        public IdentityController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<IdentityController> logger,
            IOptions<ApplicationSettings> applicationSettings)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.applicationSettings = applicationSettings.Value;
        }

        [HttpPost]
        [Route(nameof(Register))]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (model is null)
            {
                return BadRequest(model);
            }

            var user = new ApplicationUser { Email = model.Email, UserName = model.Username };
            var result = await this.userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors.FirstOrDefault());
            }

            this.logger.LogInformation("User registered.");

            return this.Ok();
        }

        [HttpPost]
        [Route(nameof(Login))]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (model is null)
            {
                return BadRequest(model);
            }

            var result = await this.signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (!result.Succeeded)
            {
                this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                return this.BadRequest(this.ModelState.GetFirstError());
            }

            var user = await this.userManager.FindByNameAsync(model.Username);
            if (user is null)
            {
                return this.BadRequest(model.Username);
            }

            this.logger.LogInformation("User logged in.");

            var securityToken = await this.GenerateToken(user);

            return this.Ok(securityToken);
        }

        private async Task<JwtModel> GenerateToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.applicationSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email)
            };

            var roles = await this.userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedToken = tokenHandler.WriteToken(token);

            return new JwtModel
            {
                Token = encryptedToken,
                Roles = roles,
                Expiration = token.ValidTo
            };
        }
    }
}
