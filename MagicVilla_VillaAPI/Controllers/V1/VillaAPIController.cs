
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MagicVilla_VillaAPI.Controllers.V1
{

    
    
    [Route("api/v{version:apiVersion}/VillaAPI")]
    [ApiController]
    [ApiVersion("1.0")]

     


    public class VillaAPIController : ControllerBase
    {
        private readonly IVillaRepository _dbVilla;
        private readonly APIResponse _apiResponse;
        private readonly IMapper _mapper;
        private readonly IImageRepository _imageRepository;
        public VillaAPIController(IVillaRepository dbVilla, IMapper mapper, IImageRepository imageRepository)
        {
            _dbVilla = dbVilla;
            _mapper = mapper;
            _apiResponse = new();
            _imageRepository = imageRepository;
        }

      


        [HttpGet]
        [Authorize(Roles ="admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(CacheProfileName = "Default30")]
        
        public async Task<ActionResult<APIResponse>> GetVillas(
            [FromQuery] string? filterBy, [FromQuery] string? search, [FromQuery] string? sortby, [FromQuery] string? sortorder = "desc"  , int pagesize = 10, int pagenumber = 1)
        {
           
            try
            {
                IQueryable<Villa> villaList;
               
                   villaList = _dbVilla.GetAllAsync(pagesize: pagesize, pagenumber: pagenumber);
                
                if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(search))
                {
                    switch(filterBy.ToLower())
                    {
                        case "name":
                            villaList = villaList.Where(u => u.Name.ToLower().Contains(search.ToLower()));
                            break;
                        
                        case "details":
                            villaList = villaList.Where(u => u.Details.ToLower().Contains(search.ToLower()));
                            break;

                        case "rate":
                            if(double.TryParse(search, out double rate))
                            {
                                villaList = villaList.Where(u => u.Rate == rate);
                            }
                           
                            break;

                        case "minrate":
                            if (double.TryParse(search, out double minrate))
                            {
                                villaList = villaList.Where(u => u.Rate >= minrate);
                            }

                            break;

                        case "maxrate":
                            if (double.TryParse(search, out double maxrate))
                            {
                                villaList = villaList.Where(u => u.Rate <= maxrate);
                            }

                            break;
                        case "occupancy":
                            if (int.TryParse(search, out int occupancy))
                            {
                                villaList = villaList.Where(u => u.Occupancy == occupancy);
                            }

                            break;

                    }
                }

                if (!string.IsNullOrEmpty(sortby))
                {
                    var isdescending = sortorder?.ToLower() == "desc";
                    villaList = sortby.ToLower() switch
                    {
                        "name" => isdescending ? villaList.OrderByDescending(u => u.Name) : villaList.OrderBy(u => u.Name),
                        "rate" => isdescending ? villaList.OrderByDescending(u => u.Rate) : villaList.OrderBy(u => u.Rate),
                        "sqft" => isdescending ? villaList.OrderByDescending(u => u.Sqft) : villaList.OrderBy(u => u.Sqft),
                        "occupancy" => isdescending ? villaList.OrderByDescending(u => u.Occupancy) : villaList.OrderBy(u => u.Occupancy),
                        "id" => isdescending ? villaList.OrderByDescending(u => u.Id) : villaList.OrderBy(u => u.Id),
                        _ => villaList.OrderBy(u => u.Id)

                    };
                  

                }
                else
                {
                    villaList = villaList.OrderBy(u => u.Id);
                }
            
                    Pagination pagination = new() { PageNumber = pagenumber, PageSize = pagesize };
                var totalcount =  await villaList.CountAsync();
                var totalpage = (int)Math.Ceiling((double)totalcount / pagesize);
                var messagebuilder = new StringBuilder();
                messagebuilder.Append($"Succefully retreived: {villaList.Count()} villa(s)");
                messagebuilder.Append($" Page: {pagenumber} of {totalpage}, {totalcount} total records");
                if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(search))
                {
                    messagebuilder.Append($" {filterBy} : {search}");
                }

                if (!string.IsNullOrEmpty(sortby) )
                {
                    messagebuilder.Append($" sorted by {sortby} : '{sortorder?.ToLower() ?? "asc"}'");
                }

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));
              var finallist=  await villaList.ToListAsync();  
                _apiResponse.Result = _mapper.Map<List<VillaDTO>>(finallist);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.DescriptiveMessage = messagebuilder.ToString();
                return Ok(_apiResponse);
            }
            catch(Exception ex) 
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.ToString() };
             
            }
            return _apiResponse;

        }

        [HttpGet("id:int", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
  
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            if (id == 0)
            {
                //_logger.Log($"Get Villa Error with id: {id} ","Error" );
                return BadRequest();
            }
            var villa = await _dbVilla.GetAsync(u => u.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            try
            {
                _apiResponse.Result = _mapper.Map<VillaDTO>(villa);
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
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromForm] VillaCreateDTO CreateDTO)
        {
            try
            {
               
                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == CreateDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("CustomError", "Villa already exists!");
                    return BadRequest(ModelState);
                }
                if (CreateDTO == null)
                {
                    return BadRequest(CreateDTO);
                }
          
                if (CreateDTO.Image != null)
                {
                    if (!_imageRepository.ValidateImage(CreateDTO.Image))
                    {
                        return BadRequest("Invalid Image File Allowe Formate: { \".jpg\", \".jpeg\", \".png\"} and Max Size Is 5 MB");
                    }
                    CreateDTO.ImageUrl = await _imageRepository.UploadImageAsync(CreateDTO.Image);
                }
                Villa model = _mapper.Map<Villa>(CreateDTO);

                await _dbVilla.CreateAsync(model);

                _apiResponse.Result = _mapper.Map<VillaNumberDTO>(model);
                _apiResponse.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = model.Id }, _apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.ToString() };

            }
            return _apiResponse;
        }
        [HttpDelete("id:int", Name = "DeleteVilla")]
        
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(villa.ImageUrl) )
                { 
                    await _imageRepository.DeleteImageAsync(villa.ImageUrl);
                }
                await _dbVilla.RemoveAsync(villa);

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
        [HttpPut("id:int", Name = "UpdateVilla")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromForm] VillaUpdateDTO UpdateDTO)
        {
            try
            {
                if (UpdateDTO == null || id != UpdateDTO.Id)
                {
                    return BadRequest();
                }
                var model = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);
                if (model == null)
                {
                    return NotFound();
                }

                if (UpdateDTO.Image != null)
                {
                    if (!_imageRepository.ValidateImage(UpdateDTO.Image))
                    {
                        return BadRequest("Invalid Image File Allowe Formate: { \".jpg\", \".jpeg\", \".png\"} and Max Size Is 5 MB");
                    }
                    UpdateDTO.ImageUrl = await _imageRepository.UploadImageAsync(UpdateDTO.Image);
                }


                


                var oldimageurl = model.ImageUrl;

                model = _mapper.Map<Villa>(UpdateDTO);
             

                if (UpdateDTO.Image != null)
                {
                    model.ImageUrl = await _imageRepository.UploadImageAsync(UpdateDTO.Image);

                    if (!string.IsNullOrEmpty(oldimageurl) && oldimageurl != model.ImageUrl )
                    {
                        await _imageRepository.DeleteImageAsync(oldimageurl);
                    }
                }
               
                await _dbVilla.UpdateAsync(model);
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

      
        [HttpPatch("id:int", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);

            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);
       
            if (villa == null)
            {
                return NotFound();
            }
            patchDTO.ApplyTo(villaDTO, ModelState);
            Villa model = _mapper.Map<Villa>(villaDTO);
          
            await _dbVilla.UpdateAsync(model);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();

        }
    }
}
