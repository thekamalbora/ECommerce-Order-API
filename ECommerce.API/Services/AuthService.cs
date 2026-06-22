using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using ECommerce.API.Helpers;
using ECommerce.API.Repositories;

namespace ECommerce.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;

        private readonly JwtTokenGenerator _jwt;

        public AuthService(
            IUserRepository repo,
            JwtTokenGenerator jwt)
        {
            _repo = repo;

            _jwt = jwt;
        }

        public async Task Register(
            RegisterDto dto)
        {
            var exists =
                await _repo
                .GetByEmail(
                    dto.Email);

            if (exists != null)
                throw new Exception(
                    "User exists");

            var user =
                new User
                {
                    Name =
                    dto.Name,

                    Email =
                    dto.Email,

                    PasswordHash =
                    BCrypt.Net.BCrypt
                    .HashPassword(
                        dto.Password),

                    CreatedDate =
                    DateTime.UtcNow
                };

            await _repo.Add(
                user);

            await _repo.Save();
        }

        public async Task<AuthResponseDto> Login(
            LoginDto dto)
        {
            var user = await _repo.GetByEmail(dto.Email);

            if (user == null) throw new Exception();

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception();

            var accessToken = _jwt.Generate(user);

            var refresh = TokenHelper.GenerateRefreshToken();

            user.RefreshToken = refresh;

            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _repo.Save();

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refresh,
                Email = user.Email
            };
        }

        public async Task<AuthResponseDto> Refresh(RefreshTokenDto dto)
        {
            var user =
                await _repo.GetByEmail(
                    dto.Email);

            if (user == null)
                throw new Exception(
                    "User not found");

            if (
                user.RefreshToken
                != dto.RefreshToken)
            {
                throw new Exception(
                    "Invalid Refresh Token");
            }

            if (
                user.RefreshTokenExpiry
                < DateTime.UtcNow)
            {
                throw new Exception(
                    "Refresh Token Expired");
            }

            // New Access Token
            var NewaccessToken =
                _jwt.Generate(
                    user);

            // Rotate Refresh Token
            var refreshToken =
                TokenHelper
                .GenerateRefreshToken();

            user.RefreshToken =
                refreshToken;

            user.RefreshTokenExpiry =
                DateTime.UtcNow
                    .AddDays(7);

            await _repo.Save();

            return new AuthResponseDto
            {
                Token =
            NewaccessToken,

                RefreshToken =
            refreshToken,

                Email =
            user.Email
            };
        }
    }
}
