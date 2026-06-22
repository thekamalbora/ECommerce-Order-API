using System.Security.Claims;

namespace ECommerce.API.Services
{
    public interface IUserService
    {
        Task<object> GetProfile(
            ClaimsPrincipal user);
    }
}
