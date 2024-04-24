using Application.Interfaces.Auth;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class UserLogoutService : IUserLogoutService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserLogoutService(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
