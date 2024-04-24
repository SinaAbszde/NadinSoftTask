using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class UserLoginService : IUserLoginService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserLoginService(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<SignInResult> LoginAsync(LoginDTO loginDto)
        {
            return await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, loginDto.RememberMe, lockoutOnFailure: false);
        }
    }
}
