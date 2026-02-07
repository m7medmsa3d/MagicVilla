using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class TokenRepository : ITokenRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        private string secretKey;
        public TokenRepository(ApplicationDbContext db, IConfiguration configuration,
            UserManager<AppicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {

            _db = db;
            _userManager = userManager;
            _mapper = mapper;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;

        }
        public async Task<string> GenerateJwtTokenAsync(AppicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var tokenhandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokendescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                         new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                         new Claim(ClaimTypes.Email, user.Email ),
                        new Claim(ClaimTypes.Name, user.Name.ToString()),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

                }),
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                
            };
            var token = tokenhandler.CreateToken(tokendescriptor);







            return tokenhandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshToken = Convert.ToBase64String(randomNumber);
            var exist = await _db.ResfreshTokens.AnyAsync(rt => rt.RefreshTokenValue == refreshToken);
            if (exist)
            {
                return await GenerateRefreshTokenAsync();
            }
            return refreshToken;

        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshTokenId)
        {
            var storedToken = await _db.ResfreshTokens.FirstOrDefaultAsync(rt => rt.RefreshTokenValue == refreshTokenId);
            if (storedToken == null)
            {
                return false;
            }
            storedToken.IsValid = false;
            await _db.SaveChangesAsync();
            return true;

        }

        public async Task SaveRefreshTokenAsync(string userId, string jwtTokenId, string refreshToken, DateTime expiresAt)
        {
            var refreshTokenEntity = new RefreshToken
            {
                UserId = userId,
                JwtTokenId = jwtTokenId,
                RefreshTokenValue = refreshToken,
                ExpiresAt = expiresAt,
                IsValid = true

            };
            await _db.ResfreshTokens.AddAsync(refreshTokenEntity);
            await _db.SaveChangesAsync();
        }

        public async Task<(bool IsValid, string? UserId, string? TokenFamilyId, bool TokenReused)> ValidateRefreshTokenAsync(string refreshToken)
        {

            var storedToken = await _db.ResfreshTokens.FirstOrDefaultAsync(rt => rt.RefreshTokenValue == refreshToken);
           
            if (storedToken == null)
            {
                return (false, null, null, false);
            }

            if (!storedToken.IsValid)
            {
                
                var tokenFamily = await _db.ResfreshTokens.Where(rt => rt.UserId == storedToken.UserId ).ToListAsync();

                if (tokenFamily.Count() > 0)
                {
                    foreach (var item in tokenFamily)
                    {
                        item.IsValid = false;
                    }
                    await _db.SaveChangesAsync();
                }
                return (false, storedToken.UserId, storedToken.JwtTokenId, true);
            }

            if (storedToken.ExpiresAt > DateTime.Now)
            {
                return (false, storedToken.UserId, storedToken.JwtTokenId, false);
            }

            return (true, storedToken.UserId, storedToken.JwtTokenId, false);
        }
    }
}
