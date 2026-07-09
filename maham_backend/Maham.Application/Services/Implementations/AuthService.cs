using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Maham.Domain.Entities;
using Maham.Domain.Interfaces;
using Maham.Application.DTOs.Auth;
using Maham.Application.Services.Interfaces;

namespace Maham.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var normalizedEmail = dto.Email.ToLower().Trim();
        var existingUser = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (existingUser != null)
        {
            throw new ArgumentException("Email is already registered.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        
        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = passwordHash,
            Role = Domain.Enums.UserRole.Member
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        var userResponse = _mapper.Map<UserResponseDto>(user);

        return new AuthResponseDto
        {
            Token = token,
            User = userResponse
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var normalizedEmail = dto.Email.ToLower().Trim();
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _tokenService.GenerateToken(user);
        var userResponse = _mapper.Map<UserResponseDto>(user);

        return new AuthResponseDto
        {
            Token = token,
            User = userResponse
        };
    }

    public async Task<UserResponseDto> GetMeAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.FullName = dto.FullName;
        if (dto.AvatarUrl != null)
        {
            user.AvatarUrl = dto.AvatarUrl;
        }
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            throw new ArgumentException("Incorrect current password.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var token = _tokenService.GenerateToken(user);
        var userResponse = _mapper.Map<UserResponseDto>(user);

        return new AuthResponseDto
        {
            Token = token,
            User = userResponse
        };
    }
}
