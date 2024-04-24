using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRegistrationService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterDTO registerDTO)
        {
            var existingUser = await _userManager.FindByNameAsync(registerDTO.UserName);
            if (existingUser != null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"Username '{registerDTO.UserName}' is already taken."
                });
            }

            var user = new ApplicationUser { UserName = registerDTO.UserName, Email = registerDTO.Email, FullName = registerDTO.FullName };
            return await _userManager.CreateAsync(user, registerDTO.Password);
        }
    }
}
