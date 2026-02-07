using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using AutoMapper;
using MagicVilla_VillaAPI.Repository.IRepository;


namespace MagicVilla_VillaAPI.Controllers.V2
{
    [Route("api/v{version:apiVersion}/VillaNumberAPI")]
    [ApiController]
  
    [ApiVersion("2.0")]
    public class VillaNumberAPIController : ControllerBase
    {

        private readonly IVillaNumberRepository _dbVillaNumber;
        private readonly IVillaRepository _dbVilla;
        private readonly APIResponse _apiResponse;
        private readonly IMapper _mapper;
        public VillaNumberAPIController(IVillaNumberRepository dbVillaNumber, IMapper mapper, IVillaRepository dbVilla)
        {
            _dbVillaNumber = dbVillaNumber;
            _mapper = mapper;
            _apiResponse = new();
            _dbVilla = dbVilla;
        }

       
        [HttpGet]
      
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

       

      
    }
}
