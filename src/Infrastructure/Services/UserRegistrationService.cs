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
            var user = new ApplicationUser { UserName = registerDTO.Email, Email = registerDTO.Email, FullName = registerDTO.FullName };
            return await _userManager.CreateAsync(user, registerDTO.Password);
        }
    }
}
