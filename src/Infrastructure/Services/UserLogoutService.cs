using Application.Interfaces.Auth;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class UserLogoutService : IUserLogoutService
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public UserLogoutService(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
