 using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using VillaApi.DTO;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/v{version:apiVersion}/UsersAuth")]
    [ApiController]
    [ApiVersionNeutral]
    
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        protected APIResponse _apiresponse;
        private readonly ITokenRepository _tokenRepository;
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepo, ITokenRepository tokenRepository, IUserRepository userRepository)
        {
            _userRepo = userRepo;
            _apiresponse = new();
            _tokenRepository = tokenRepository;
            _userRepository = userRepository;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var logiresponse = await _userRepo.Login(model);
            if(string.IsNullOrEmpty(logiresponse.AccessToken))
            {
                _apiresponse.StatusCode = HttpStatusCode.BadRequest;
               _apiresponse.ErrorMessages.Add(  "Username or password is incorrect" );
                _apiresponse.IsSuccess = false;
                return BadRequest(_apiresponse);
            }
            _apiresponse.StatusCode = HttpStatusCode.OK;
            _apiresponse.Result = logiresponse;
            _apiresponse.IsSuccess = true;
            return Ok(_apiresponse);

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
        {
           bool ifisuserisunique = _userRepo.IsUniqueUser(model.UserName);
            if (!ifisuserisunique)
            {
                _apiresponse.StatusCode = HttpStatusCode.BadRequest;
                _apiresponse.ErrorMessages.Add("Username already exists");
                _apiresponse.IsSuccess = false;
                return BadRequest(_apiresponse);
            }
            var user = await _userRepo.Register(model);
            if (user == null)
            {
                _apiresponse.StatusCode = HttpStatusCode.BadRequest;
                _apiresponse.ErrorMessages.Add("Error while registering");
                _apiresponse.IsSuccess = false;
                return BadRequest(_apiresponse);
            }
            _apiresponse.StatusCode = HttpStatusCode.OK;
            _apiresponse.IsSuccess = true;
            return Ok(_apiresponse);
        }



        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RefreshAccessToken([FromBody] RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            try
            {


                if (refreshTokenRequestDTO == null || string.IsNullOrEmpty(refreshTokenRequestDTO.RefreshToken))
                {
                    _apiresponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiresponse.ErrorMessages.Add("Refresh token is requird");
                    _apiresponse.IsSuccess = false;
                    return BadRequest(_apiresponse);
                }
                var tokenResponse = await _userRepository.RefreshAccessTokenAsync(refreshTokenRequestDTO);
                if (tokenResponse == null)
                {
                    _apiresponse.StatusCode = HttpStatusCode.Unauthorized;
                    _apiresponse.ErrorMessages.Add("Invalid or expired refresh token");
                    _apiresponse.IsSuccess = false;
                    return BadRequest(_apiresponse);
                }
                _apiresponse.StatusCode = HttpStatusCode.OK;
                _apiresponse.Result = tokenResponse;
                _apiresponse.IsSuccess = true;
                return Ok(_apiresponse);

            }
            catch (Exception ex)
            {
                _apiresponse.StatusCode = HttpStatusCode.InternalServerError;
                _apiresponse.ErrorMessages.Add(ex.Message);
                _apiresponse.IsSuccess = false;
                return BadRequest(_apiresponse);
            }
        }

    }






}
