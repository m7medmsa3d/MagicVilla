using System.ComponentModel.DataAnnotations;

namespace MagicVilla_VillaAPI.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public string JwtTokenId { get; set; } = default!;
        public string RefreshTokenValue { get; set; } = default!;
        public bool IsValid { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
