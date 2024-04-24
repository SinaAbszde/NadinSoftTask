using Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Auth
{
    public interface IUserLoginService
    {
        Task<SignInResult> LoginAsync(LoginDTO loginDto);
    }
}
