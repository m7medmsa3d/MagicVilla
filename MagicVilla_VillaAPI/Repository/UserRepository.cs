using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using MagicVilla_VillaAPI.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VillaApi.DTO;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ITokenRepository _tokenRepository;
        private string secretKey;
        
        public UserRepository(ApplicationDbContext db, IConfiguration configuration,
             UserManager<AppicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, ITokenRepository tokenRepository)
        {

            _db = db;
            _userManager = userManager;
            _mapper = mapper;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
            _tokenRepository = tokenRepository;
        }

        public bool IsUniqueUser(string username)
        {
            var user = _db.AppicationUsers.FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _db.AppicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());
            bool isvalid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
            if (user == null || isvalid == false)
            {
                return new TokenDTO
                {
                   
                    AccessToken = ""
                };
            }
         
            else
            {

                var token = await _tokenRepository.GenerateJwtTokenAsync(user);
                var tokenhandler = new JwtSecurityTokenHandler();
                var jwttoken = tokenhandler.ReadJwtToken(token);
                var jwttokenId  = jwttoken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                var refreshToken = await _tokenRepository.GenerateRefreshTokenAsync();
                var refreshTokenExpires = DateTime.UtcNow.AddMinutes(3);
                await _tokenRepository.SaveRefreshTokenAsync(user.Id, jwttokenId, refreshToken, refreshTokenExpires);


                TokenDTO loginresponseDTO = new()
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt=jwttoken.ValidTo


                };
                return loginresponseDTO;
            }

        }

       

        public async Task<UserDTO> Register(RegisterationRequestDTO registerationRequest)
        {
            AppicationUser user = new()
            {
                UserName = registerationRequest.UserName,
                Name = registerationRequest.Name,
                NormalizedEmail = registerationRequest.Name.ToUpper(),
                Email = registerationRequest.UserName,
                EmailConfirmed = true
            };
            try
            {
               
                var result = await _userManager.CreateAsync(user, registerationRequest.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole("admin"));
                        await _roleManager.CreateAsync(new IdentityRole("customer"));
                    }
                    if (string.IsNullOrEmpty(registerationRequest.Role))
                    {
                        await _userManager.AddToRoleAsync(user, "customer");
                    }
                    else 
                    {
                        await _userManager.AddToRoleAsync(user, registerationRequest.Role);
                    }
                   
                    var usertoreturn = _db.AppicationUsers.FirstOrDefault(u => u.UserName == registerationRequest.UserName);
                    return _mapper.Map<UserDTO>(usertoreturn);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex) 
            {
                throw;
            }
            return new UserDTO();
        }



        public async Task<TokenDTO?> RefreshAccessTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshTokenRequestDTO.RefreshToken))
                {
                    return null;
                }

              
                var (isValid, userId, tokenFamilyId, tokenReused) = await _tokenRepository.ValidateRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);

               
                if (tokenReused)
                {
                    return null;
                }

                
                if (!isValid || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenFamilyId))
                {
                    return null;
                }

                
                var user = await _db.AppicationUsers.FindAsync(userId);
                if (user == null)
                {
                    return null;
                }

               
                await _tokenRepository.RevokeRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);

              
                var token = await _tokenRepository.GenerateJwtTokenAsync(user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

             
                var newRefreshToken = await _tokenRepository.GenerateRefreshTokenAsync();
                var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(5);

                await _tokenRepository.SaveRefreshTokenAsync(user.Id, tokenFamilyId, newRefreshToken, refreshTokenExpiry);

                TokenDTO tokenDTO = new TokenDTO
                {

                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = jwtToken.ValidTo
                };
                return tokenDTO;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred during token refresh", ex);
            }
        }
    }

    }

