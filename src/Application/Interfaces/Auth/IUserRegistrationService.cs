using Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Auth
{
    public interface IUserRegistrationService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterDTO userRegistration);
    }
}