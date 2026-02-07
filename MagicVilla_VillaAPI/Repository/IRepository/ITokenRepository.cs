using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface ITokenRepository
    {
         Task<string> GenerateJwtTokenAsync(AppicationUser user);

        Task<string> GenerateRefreshTokenAsync( );

        Task<bool> RevokeRefreshTokenAsync(string refreshTokenId);

        Task SaveRefreshTokenAsync(string userId, string jwtTokenId, string refreshToken, DateTime expiresAt);

        Task<(bool IsValid, string? UserId, string? TokenFamilyId, bool TokenReused)> ValidateRefreshTokenAsync(string refreshToken);
    }
}
