using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.Auth;
using HerreraSystem.Application.DTOs.UserDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<ServiceResult<IEnumerable<UserDto>>> GetAllUsersAsync();
        Task<ServiceResult<UserDto>> GetByIdAsync(int id);
        Task<ServiceResult<string>> CreateAsync(CreateUserDto dto);
        Task<ServiceResult<string>> UpdateAsync(UpdateUserDto dto);
        Task<ServiceResult<string>> ToggleStatusAsync(int targetUserId, int currentLoggedInUserId);
        Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(ForgotPasswordRequestDto dto);
        Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordRequestDto dto);
        Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordDto dto);

    }
}
