using System.Security.Cryptography;
using Hangfire;
using LibraryManagementSystem.Application.DTOs.AuthDtos;
using LibraryManagementSystem.Application.Interfaces;
using LibraryManagementSystem.Application.Interfaces.IRepositories;
using LibraryManagementSystem.Application.Interfaces.IServices;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Infrastructure.Services;
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJob;

    public AuthService(UserManager<ApplicationUser> userManager,
        IJwtService jwtService, 
        IUnitOfWork unitOfWork,
        IBackgroundJobClient backgroundJob)
    {
        this._userManager = userManager;
        this._jwtService = jwtService;
        this._unitOfWork = unitOfWork;
        this._backgroundJob = backgroundJob;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {

        //var validationResult = await _registerValidator.ValidateAsync(dto);
        //if (!validationResult.IsValid)
        //{
        //    return new AuthResponseDto
        //    {
        //        IsAuthenticated = false,
        //        Message = "Validation failed: " + string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
        //    };
        //}

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);

        if (existingUser is not null)
        {
            return new AuthResponseDto
            {
                Message = "Email is already registered."
            };
        }


        var existingUsername = await _userManager.FindByNameAsync(dto.Username);
        if (existingUsername is not null)
        {
            return new AuthResponseDto
            {
                Message = "UserName is already Exsit."
            };
        }


        var user = new ApplicationUser
        {
            Name = dto.Name,
            UserName = dto.Username,
            Email = dto.Email,
            EmailConfirmed = false,
            PhoneNumber = dto.PhoneNumber
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);


        if (!createResult.Succeeded)
        {
            return new AuthResponseDto
            {
                Message = $"User creation failed:{string.Join(',', createResult.Errors.Select(e => e.Description))}"
            };
        }

        var roleResult = await _userManager.AddToRoleAsync(user, SeedRoles.Member.Name!);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return new AuthResponseDto
            {
                Message = $"Failed to assign role: {string.Join(',', roleResult.Errors.Select(e => e.Description))}"
            };
        }

        user.EmailConfirmationCode = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        user.EmailConfirmationCodeExpiresAt = DateTime.UtcNow.AddMinutes(3);

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return new AuthResponseDto
            {
                Message = "Failed to generate email confirmation code."
            };
        }

        var emailBody = WriteRegistrationEmailBody(user);

        //await _emailService.SendAsync(user.Email, "Library - Verify your email", emailBody);
        _backgroundJob.Enqueue<IEmailService>(x => x.SendAsync(user.Email, "Library - Verify your email", emailBody));

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "User registered successfully. Please check your email for the confirmation code.",
            UserName = user.UserName,
            Email = user.Email
        };
    }
    public async Task<AuthResponseDto> ConfirmEmailAsync(ConfirmEmailDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null)
        {
            return new AuthResponseDto
            {
                Message = "Invalid email or confirmation code."
            };
        }

        if (user.EmailConfirmed)
        {
            return new AuthResponseDto
            {
                Message = "Email is already confirmed."
            };
        }


        var isCodeValid =
              !string.IsNullOrWhiteSpace(user.EmailConfirmationCode) &&
              user.EmailConfirmationCode == dto.Code &&
              user.EmailConfirmationCodeExpiresAt.HasValue &&
              user.EmailConfirmationCodeExpiresAt.Value > DateTime.UtcNow;

        if (!isCodeValid)
        {
            user.EmailConfirmationCodeAttempts++;

            if (user.EmailConfirmationCodeAttempts >= 5)
            {
                user.EmailConfirmationCode = null;
                user.EmailConfirmationCodeExpiresAt = null;
                user.EmailConfirmationCodeAttempts = 0;

                await _userManager.UpdateAsync(user);

                return new AuthResponseDto
                {
                    Message = "Too many invalid attempts. Please request a new confirmation code."
                };
            }

            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Message = "Invalid or expired confirmation code."
            };
        }


        user.EmailConfirmed = true;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationCodeExpiresAt = null;
        user.EmailConfirmationCodeAttempts = 0;
        user.IsActive = true;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                Message = "Failed to confirm email."
            };
        }

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Email confirmed successfully."
        };
    }
    public async Task<AuthResponseDto> ResendConfirmationAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || user.EmailConfirmed)
        {
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "If this email is registered and not confirmed, a new confirmation code has been sent."
            };
        }

        user.EmailConfirmationCode = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        user.EmailConfirmationCodeExpiresAt = DateTime.UtcNow.AddMinutes(3);
        user.EmailConfirmationCodeAttempts = 0;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Failed to generate a new confirmation code."
            };
        }

        var emailBody = WriteRegistrationEmailBody(user);

       // await _emailService.SendAsync(user.Email!, "Library - Verify your email", emailBody);
        _backgroundJob.Enqueue<IEmailService>(x => x.SendAsync(user.Email!, "Library - Verify your email", emailBody));

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "A new confirmation code has been sent to your email.",
            UserName = user.UserName,
            Email = user.Email
        };
    }
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        //var validationResult = await _loginValidator.ValidateAsync(dto);
        //if (!validationResult.IsValid)
        //{
        //    return new AuthResponseDto
        //    {
        //        Message = "Invalid login credentials."
        //    };
        //}

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return new AuthResponseDto
            {
                Message = "Invalid email or password."
            };
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
        {
            return new AuthResponseDto
            {
                Message = "Invalid email or password."
            };
        }

        if (!user.IsActive)
        {
            return new AuthResponseDto
            {
                Message = "Please confirm your email before logging in."
            };
        }

        var tokenResult = await _jwtService.CreateJwtTokenAsync(user);
        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Login successful.",
            Token = tokenResult.Token,
            RefreshToken = tokenResult.RefreshToken,
            ExpireAt = tokenResult.ExpireAt
        };
    }
    public async Task<AuthResponseDto> LogOutAsync(string refreshToken)
    {
        var storedToken = await _unitOfWork._RefreshTokenRepo.GetByTokenAsync(refreshToken);

        if (storedToken is null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Invalid refresh token."
            };
        }

        if (storedToken.IsRevoked)
        {
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Logged out successfully."
            };
        }

        storedToken.IsRevoked = true;
        await _unitOfWork.SaveChangesAsync();
        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Logged out successfully."
        };
    }
    public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
    {
        //var validate = await _resetPasswordValidator.ValidateAsync(dto);
        //if (!validate.IsValid)
        //{
        //    return new AuthResponseDto
        //    {
        //        Message = "Validation failed: " + string.Join(", ", validate.Errors.Select(e => e.ErrorMessage))
        //    };
        //}

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return new AuthResponseDto
            {
                Message = "Invalid email or Reset code."
            };
        }

        if (user.PasswordResetCodeAttempts >= 5)
        {
            return new AuthResponseDto
            {
                Message = "Too many failed reset attempts. Please request a new code."
            };
        }

        var isCodeValid =
            !string.IsNullOrWhiteSpace(user.PasswordResetCode) &&
            user.PasswordResetCode == dto.Code &&
            user.PasswordResetCodeExpiresAt.HasValue &&
            user.PasswordResetCodeExpiresAt.Value > DateTime.UtcNow;

        if (!isCodeValid)
        {
            user.PasswordResetCodeAttempts++;

            if (user.PasswordResetCodeAttempts >= 5)
            {
                user.PasswordResetCode = null;
                user.PasswordResetCodeExpiresAt = null;
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Failed to update reset attempts." };
            }

            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Reset code has expired or is invalid."
            };
        }

        var removePasswordResult = await _userManager.RemovePasswordAsync(user);

        if (!removePasswordResult.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Failed to reset password."
            };
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);

        if (!addPasswordResult.Succeeded)
        {
            return new AuthResponseDto { IsSuccess = false, Message = $"Failed to reset password: {string.Join(", ", addPasswordResult.Errors.Select(e => e.Description))}" };
        }

        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiresAt = null;
        user.PasswordResetCodeAttempts = 0;
        var updateUserResult = await _userManager.UpdateAsync(user);

        if (!updateUserResult.Succeeded)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Password was changed, but failed to clear reset code." };
        }


        return new AuthResponseDto { IsSuccess = true, Message = "Password reset successfully." };
    }
    public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null)
        {
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "If the email is registered, a password reset code has been sent."
            };
        }

        if (!user.EmailConfirmed)
        {
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "If the email is registered, a password reset code has been sent."
            };
        }

        var resetCode = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        user.PasswordResetCode = resetCode;
        user.PasswordResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(3);
        user.PasswordResetCodeAttempts = 0;


        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Failed to generate password reset code."
            };
        }

        var emailBody = WritePasswordResetEmailBody(user);

        //await _emailService.SendAsync(user.Email!, "Library - Password Reset Code", emailBody);
        _backgroundJob.Enqueue<IEmailService>(x => x.SendAsync(user.Email!, "Library - Password Reset Code", emailBody));

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "If the email is registered, a password reset code has been sent."
        };

    }
    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto payload)
    {
        var dbRefreshToken = await _unitOfWork._RefreshTokenRepo.GetByTokenAsync(payload.RefreshToken);

        if (dbRefreshToken is null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Invalid refresh token."
            };
        }

        if (dbRefreshToken.IsUsed || dbRefreshToken.IsRevoked)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Refresh token is no longer valid. Please sign in again."
            };
        }

        if (dbRefreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Refresh token has expired. Please sign in again."
            };
        }

        var user = await _userManager.FindByIdAsync(dbRefreshToken.UserId);

        if (user is null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "User no longer exists."
            };
        }

        if (!user.IsActive)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "User is deactivated."
            };
        }

        dbRefreshToken.IsUsed = true;
        dbRefreshToken.IsRevoked = true;
        await _unitOfWork.SaveChangesAsync();

        var newTokenResponse = await _jwtService.CreateJwtTokenAsync(user);

        return new AuthResponseDto
        {
            IsSuccess = true,

            Message = "Token refreshed successfully.",
            Token = newTokenResponse.Token,
            RefreshToken = newTokenResponse.RefreshToken,
            ExpireAt = newTokenResponse.ExpireAt
        };
    }
    public async Task<AuthResponseDto> AddStaffByAdminAsync(CreateUserDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);

        if (existingUser is not null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Email is already registered."
            };
        }

        var existingUsername = await _userManager.FindByNameAsync(dto.UserName);

        if (existingUsername is not null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "UserName already exists."
            };
        }

        var staff = new ApplicationUser
        {
            Name = dto.Name,
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            EmailConfirmed = false,
            IsActive = false
        };

        var createResult = await _userManager.CreateAsync(staff, dto.Password);

        if (!createResult.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = $"Staff creation failed: {string.Join(", ", createResult.Errors.Select(e => e.Description))}"
            };
        }

        var roleResult = await _userManager.AddToRoleAsync(staff, SeedRoles.Staff.Name);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(staff);

            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = $"Failed to assign Staff role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}"
            };
        }

        var confirmationCode = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

        staff.EmailConfirmationCode = confirmationCode;
        staff.EmailConfirmationCodeExpiresAt = DateTime.UtcNow.AddMinutes(10);
        staff.EmailConfirmationCodeAttempts = 0;

        var updateResult = await _userManager.UpdateAsync(staff);

        if (!updateResult.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Staff was created, but confirmation code could not be generated."
            };
        }

        var emailBody = WriteRegistrationEmailBody(staff);

         // await _emailService.SendAsync(.Email!, "Library - Verify your email", emailBody);
        _backgroundJob.Enqueue<IEmailService>(x => x.SendAsync(staff.Email!, "Library - Verify your Email", emailBody));

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Staff account created successfully. A confirmation code has been sent to the staff email.",
            UserName = staff.UserName,
            Email = staff.Email
        };
    }
    private string WriteRegistrationEmailBody(ApplicationUser user)
    {
        return $"""
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</head>

<body style="
    margin: 0;
    padding: 0;
    background-color: #f4f6f8;
    font-family: Arial, Helvetica, sans-serif;
    color: #333333;
">

    <div style="
        max-width: 600px;
        margin: 40px auto;
        background-color: #ffffff;
        border-radius: 12px;
        overflow: hidden;
        box-shadow: 0 2px 10px rgba(0,0,0,0.08);
    ">

        <!-- Header -->
        <div style="
            background-color: #2563eb;
            padding: 25px;
            text-align: center;
        ">
            <h1 style="
                margin: 0;
                color: #ffffff;
                font-size: 24px;
            ">
                Bib_Alex_Project
            </h1>
        </div>

        <!-- Content -->
        <div style="padding: 40px 35px;">

            <h2 style="
                margin-top: 0;
                color: #222222;
            ">
                Welcome, {user.Name}! 👋
            </h2>

            <p style="
                font-size: 16px;
                line-height: 1.6;
                color: #555555;
            ">
                Thanks for creating an account with us.
                Please use the verification code below to confirm your email address.
            </p>

            <!-- Verification Code -->
            <div style="
                margin: 30px 0;
                padding: 20px;
                background-color: #f1f5f9;
                border-radius: 8px;
                text-align: center;
            ">
                <p style="
                    margin: 0 0 10px;
                    font-size: 13px;
                    color: #64748b;
                ">
                    YOUR VERIFICATION CODE
                </p>

                <div style="
                    font-size: 32px;
                    font-weight: bold;
                    letter-spacing: 8px;
                    color: #2563eb;
                ">
                    {user.EmailConfirmationCode}
                </div>
            </div>

            <p style="
                font-size: 14px;
                line-height: 1.6;
                color: #64748b;
            ">
                This code will expire in <strong>3 minutes</strong>.
                If you didn't create this account, you can safely ignore this email.
            </p>

            <p style="
                margin-top: 30px;
                font-size: 15px;
                color: #555555;
            ">
                Best regards,<br>
                <strong>Bib_Alex_Project Team</strong>
            </p>

        </div>

        <!-- Footer -->
        <div style="
            padding: 20px;
            background-color: #f8fafc;
            text-align: center;
            font-size: 12px;
            color: #94a3b8;
        ">
            <p style="margin: 0;">
                © 2026 Bib_Alex_Project. All rights reserved.
            </p>
        </div>

    </div>

</body>
</html>
""";
    }
    private string WritePasswordResetEmailBody(ApplicationUser user)
    {
        return $"""
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</head>

<body style="
    margin: 0;
    padding: 0;
    background-color: #f4f6f8;
    font-family: Arial, Helvetica, sans-serif;
    color: #333333;
">

    <div style="
        max-width: 600px;
        margin: 40px auto;
        background-color: #ffffff;
        border-radius: 12px;
        overflow: hidden;
        box-shadow: 0 2px 10px rgba(0,0,0,0.08);
    ">

        <!-- Header -->
        <div style="
            background-color: #2563eb;
            padding: 25px;
            text-align: center;
        ">
            <h1 style="
                margin: 0;
                color: #ffffff;
                font-size: 24px;
            ">
                Bib_Alex_Project
            </h1>
        </div>

        <!-- Content -->
        <div style="padding: 40px 35px;">

            <h2 style="
                margin-top: 0;
                color: #222222;
            ">
                Password Reset Request
            </h2>

            <p style="
                font-size: 16px;
                line-height: 1.6;
                color: #555555;
            ">
                Hello {user.Name},
            </p>

            <p style="
                font-size: 16px;
                line-height: 1.6;
                color: #555555;
            ">
                We received a request to reset the password for your
                Bib_Alex_Project account. Use the code below to reset your password.
            </p>

            <!-- Reset Code -->
            <div style="
                margin: 30px 0;
                padding: 20px;
                background-color: #f1f5f9;
                border-radius: 8px;
                text-align: center;
            ">
                <p style="
                    margin: 0 0 10px;
                    font-size: 13px;
                    color: #64748b;
                ">
                    PASSWORD RESET CODE
                </p>

                <div style="
                    font-size: 32px;
                    font-weight: bold;
                    letter-spacing: 8px;
                    color: #2563eb;
                ">
                    {user.PasswordResetCode}
                </div>
            </div>

            <p style="
                font-size: 14px;
                line-height: 1.6;
                color: #64748b;
            ">
                This code will expire in <strong>10 minutes</strong>.
                For your security, do not share this code with anyone.
            </p>

            <p style="
                font-size: 14px;
                line-height: 1.6;
                color: #64748b;
            ">
                If you did not request a password reset, you can safely ignore
                this email. Your password will remain unchanged.
            </p>

            <p style="
                margin-top: 30px;
                font-size: 15px;
                color: #555555;
            ">
                Best regards,<br>
                <strong>Bib_Alex_Project Team</strong>
            </p>

        </div>

        <!-- Footer -->
        <div style="
            padding: 20px;
            background-color: #f8fafc;
            text-align: center;
            font-size: 12px;
            color: #94a3b8;
        ">
            <p style="margin: 0;">
                © 2026 Bib_Alex_Project. All rights reserved.
            </p>
        </div>

    </div>

</body>
</html>
""";
    }


}
