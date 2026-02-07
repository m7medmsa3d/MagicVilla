using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Identity.Data;
using VillaApi.DTO;


namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);
        Task<TokenDTO> Login(LoginRequestDTO loginRequest);
        Task<UserDTO> Register(RegisterationRequestDTO registerationRequest);

        Task<TokenDTO?> RefreshAccessTokenAsync(RefreshTokenRequestDTO refreshTokenDTO);
    }
}
