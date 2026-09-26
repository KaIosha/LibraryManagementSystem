using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Application.Interfaces;
using LibraryManagementSystem.Application.Interfaces.IRepositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LibraryManagementSystem.Infrastructure.Services
{
    public class JwtService : IJwtService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwt;

        public JwtService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwt)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _jwt = jwt.Value;
        }
        public async Task<JwtResponseDto> CreateJwtTokenAsync(ApplicationUser user)
        {
       
            var jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name!),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            };
            var userRoles = await _userManager.GetRolesAsync(user);
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));



            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                JwtId = jti, 
                Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString(),
                IsUsed = false,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays)
            };

            await _unitOfWork._RefreshTokenRepo.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            return new JwtResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken.Token,
                ExpireAt = token.ValidTo
            };
        }
    }
}