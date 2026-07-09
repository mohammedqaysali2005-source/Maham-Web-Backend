using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Maham.Domain.Entities;
using Maham.Domain.Interfaces;
using Maham.Domain.Enums;
using Maham.Application.DTOs.Auth;
using Maham.Application.Services.Interfaces;

namespace Maham.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileService _fileService;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileService = fileService;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(Guid currentUserId)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
        if (currentUser == null || currentUser.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Only Admins can retrieve the full user list.");
        }

        var users = await _unitOfWork.Users.GetAllAsync();
        return _mapper.Map<IEnumerable<UserResponseDto>>(users);
    }

    public async Task<UserResponseDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<IEnumerable<UserResponseDto>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<UserResponseDto>();
        }

        var lowerQuery = query.ToLower();
        var users = await _unitOfWork.Users.Query()
            .Where(u => u.FullName.ToLower().Contains(lowerQuery) || u.Email.ToLower().Contains(lowerQuery))
            .Take(20)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserResponseDto>>(users);
    }

    public async Task<UserResponseDto> UploadAvatarAsync(Guid userId, IFormFile file)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Delete old avatar if present
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            _fileService.DeleteFile(user.AvatarUrl);
        }

        var avatarUrl = await _fileService.SaveFileAsync(file, "uploads/avatars");
        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> UpdateUserRoleAsync(Guid userId, string newRole, Guid currentUserId)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
        if (currentUser == null || currentUser.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Only Admins can update user roles.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (Enum.TryParse<UserRole>(newRole, true, out var roleEnum))
        {
            user.Role = roleEnum;
            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task DeleteUserAsync(Guid userId, Guid currentUserId)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
        if (currentUser == null || currentUser.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Only Admins can delete users.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Delete avatar if exists
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            _fileService.DeleteFile(user.AvatarUrl);
        }

        _unitOfWork.Users.Delete(user);
        await _unitOfWork.SaveChangesAsync();
    }
}
