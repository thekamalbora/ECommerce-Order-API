using System.Security.Claims;

namespace ECommerce.API.Services
{
    public class UserService : IUserService
    {
        public async Task<object>
        GetProfile(
            ClaimsPrincipal user)
        {
            var name =
                user.FindFirst(
                    ClaimTypes.Name)
                ?.Value;

            var email =
                user.FindFirst(
                    ClaimTypes.Email)
                ?.Value;

            var role =
                user.FindFirst(
                    ClaimTypes.Role)
                ?.Value;

            return await Task.FromResult(
            new
            {
                Name = name,
                Email = email,
                Role = role
            });
        }
    }
}
