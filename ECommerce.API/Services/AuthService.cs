using System;
using System.Threading.Tasks;
using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using ECommerce.API.Helpers;
using ECommerce.API.Repositories;

namespace ECommerce.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly JwtTokenGenerator _jwt;

    public AuthService(IUserRepository repo, JwtTokenGenerator jwt)
    {
        _repo = repo;
        _jwt = jwt;
    }

    public async Task Register(RegisterDto dto)
    {
        // Check if a user with this email address already exists in the system
        var exists = await _repo.GetByEmail(dto.Email);
        if (exists != null)
        {
            throw new Exception("User exists");
        }

        // Initialize a new User entity and securely hash the plain text password
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedDate = DateTime.UtcNow
        };

        // Persist the new account securely to the database store
        await _repo.Add(user);
        await _repo.Save();
    }

    public async Task<AuthResponseDto> Login(LoginDto dto)
    {
        // Find the user account associated with the incoming email request
        var user = await _repo.GetByEmail(dto.Email);
        if (user == null)
        {
            throw new Exception("Invalid login credentials");
        }

        // Verify the provided raw text password matches our stored hash securely
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new Exception("Invalid login credentials");
        }

        // Issue a short-lived cryptographically signed JWT access token 
        var accessToken = _jwt.Generate(user);

        // Generate a cryptographically secure string token for session persistence
        var refresh = TokenHelper.GenerateRefreshToken();

        // Update the user profile session parameters with the new token details
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
        // Find the user associated with the tracking session renewal payload
        var user = await _repo.GetByEmail(dto.Email);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        // Confirm the client's payload token matches the one stored on the server
        if (user.RefreshToken != dto.RefreshToken)
        {
            throw new Exception("Invalid Refresh Token");
        }

        // Ensure the active session window hasn't already exceeded its maximum lifetime
        if (user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            throw new Exception("Refresh Token Expired");
        }

        // Issue a brand new JWT access token to resume secure API calls
        var newAccessToken = _jwt.Generate(user);

        // Rotate the existing token with a clean new token string (Single Use Refresh Pattern)
        var refreshToken = TokenHelper.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _repo.Save();

        return new AuthResponseDto
        {
            Token = newAccessToken,
            RefreshToken = refreshToken,
            Email = user.Email
        };
    }
}