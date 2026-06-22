using ECommerce.API.DTOs;

namespace ECommerce.API.Services
{
    public interface IAuthService
    {
        Task Register(RegisterDto dto);

        Task<AuthResponseDto> Login(
            LoginDto dto);
        Task<AuthResponseDto>
Refresh(
    RefreshTokenDto dto);
    }
}
