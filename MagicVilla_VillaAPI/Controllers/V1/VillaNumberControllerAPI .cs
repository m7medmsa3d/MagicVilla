using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using AutoMapper;
using MagicVilla_VillaAPI.Repository.IRepository;


namespace MagicVilla_VillaAPI.Controllers.V1
{
    [Route("api/v{version:apiVersion}/VillaNumberAPI")]
    [ApiController]
    [ApiVersion("1.0")]
    
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

        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {
           
            try
            {
                IEnumerable<VillaNumber> villaNumberList =  _dbVillaNumber.GetAllAsync();
                _apiResponse.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumberList);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.ToString() };

            }
            return _apiResponse;

        }

       
        [HttpGet("{id:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
        {
            if (id == 0)
            {
               
                return BadRequest();
            }
            var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
            if (villaNumber == null)
            {
                return NotFound();
            }
            try
            {
                _apiResponse.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.ToString() };

            }
            return _apiResponse;

        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO CreateNumberDTO)
        {
            try
            {
               
                if (await _dbVillaNumber.GetAsync(u => u.VillaNo == CreateNumberDTO.VillaNo) != null)
                {
                    ModelState.AddModelError("CustomError", "Villa Number already exists!");
                    return BadRequest(ModelState);
                }
                if(await _dbVilla.GetAsync(u=>u.Id == CreateNumberDTO.VillID )== null)
                    {
                    ModelState.AddModelError("CustomError", "Villa ID does not exist!");
                    return BadRequest(ModelState);
                }

                if (CreateNumberDTO == null)
                {
                    return BadRequest(CreateNumberDTO);
                }
             

                VillaNumber model = _mapper.Map<VillaNumber>(CreateNumberDTO);
               
                await _dbVillaNumber.CreateAsync(model);

                _apiResponse.Result = _mapper.Map<VillaNumberDTO>(model);
                _apiResponse.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = model.VillaNo }, _apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.ToString() };

            }
            return _apiResponse;
        }
        [HttpDelete("id:int", Name = "DeleteVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }
                var villa = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
                if (villa == null)
                {
                    return NotFound();
                }
                await _dbVillaNumber.RemoveAsync(villa);

                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.ToString() };

            }
            return _apiResponse;
        }
        [HttpPut("id:int", Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO UpdateDTO)
        {
            try
            {
                if (UpdateDTO == null || id != UpdateDTO.VillaNo)
                {
                    return BadRequest();
                }
                if (await _dbVilla.GetAsync(u => u.Id == UpdateDTO.VillID) == null)
                {
                    ModelState.AddModelError("CustomError", "Villa ID does not exist!");
                    return BadRequest(ModelState);
                }
                var villa = await _dbVillaNumber.GetAsync(u => u.VillaNo == id, tracked: false);
                if (villa == null)
                {
                    return NotFound();
                }
                VillaNumber model = _mapper.Map<VillaNumber>(UpdateDTO);
           
                await _dbVillaNumber.UpdateAsync(model);
                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                _apiResponse.IsSuccess = true;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.ToString() };

            }
            return _apiResponse;

        }

      
    }
}
