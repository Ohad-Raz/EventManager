using AutoMapper;
using EventManager.DAL.Models;
using EventManager.DAL.Repositories;
using EventManager.WebAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PerformerController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IPerformerRepository _performerRepository;

        public PerformerController(IPerformerRepository performerRepository, IMapper mapper)
        {
            _performerRepository = performerRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all performers.
        /// Endpoint: GET /api/Performer
        /// </summary>
        [HttpGet]
        public ActionResult<ICollection<PerformerDto>> Get()
        {
            try
            {
                // 1. load all performers from database
                List<Performer> performers = _performerRepository.GetAllPerformers();
                // 2. map entities to DTOs
                List<PerformerDto> result = _mapper.Map<List<PerformerDto>>(performers);
                // 3. return DTO list
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Returns one performer by id.
        /// Endpoint: GET /api/Performer/{id}
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<PerformerDto> Get(int id)
        {
            try
            {
                // 1. find performer by id
                Performer? performer = _performerRepository.GetPerformerById(id);

                // 2. if not found, return NotFound
                if (performer == null) return NotFound($"Performer with id={id} was not found.");
                // 3. map entity to DTO
                PerformerDto result = _mapper.Map<PerformerDto>(performer);
                // 4. return DTO
                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Creates a new performer.
        /// Endpoint: POST /api/Performer
        /// Only Admin and Organizer are allowed to access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public ActionResult<PerformerDto> Post(PerformerDto performerDto)
        {
            try
            {
                // 1. validate model state
                if (!ModelState.IsValid) return BadRequest(ModelState);
                // 2. create new performer entity
                Performer newPerformer = _mapper.Map<Performer>(performerDto);
                // 3. save to database
                _performerRepository.AddPerformer(newPerformer);
                _performerRepository.SaveChanges();
                // 4. copy generated Id back to DTO
                performerDto.Id = newPerformer.Id;
                // 5. return created DTO
                return Ok(performerDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing performer.
        /// Endpoint: PUT /api/Performer/{id}
        /// Only Admin and Organizer are allowed to access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id}")]
        public ActionResult<PerformerDto> Put(int id, PerformerDto performerDto)
        {
            try
            {
                // 1. validate model state
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // 2. find existing performer by id
                Performer? performer = _performerRepository.GetPerformerById(id);
                // 3. if not found, return NotFound
                if (performer == null) return NotFound($"Performer with id={id} was not found.");
                // 4. update editable fields
                _mapper.Map(performerDto, performer);
                // 5. save changes
                _performerRepository.SaveChanges();
                // 6. copy id back to DTO if needed
                performerDto.Id = performer.Id;
                // 7. return updated DTO
                return Ok(performerDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a performer by id.
        /// Endpoint: DELETE /api/Performer/{id}
        /// Only Admins are allowed to access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                // 1. find existing performer by id
                Performer? performer = _performerRepository.GetPerformerById(id);
                // 2. if not found, return NotFound
                if (performer == null) return NotFound($"Performer with id={id} was not found.");

                // Block delete when performer is still assigned to events.
                if (_performerRepository.PerformerHasEventAssignments(id))
                    return BadRequest($"Performer with id={id} cannot be deleted because events use it.");

                // 3. remove performer from database
                _performerRepository.RemovePerformer(performer);
                // 4. save changes
                _performerRepository.SaveChanges();
                // 5. return success response
                return Ok($"Performer with id={id} has been deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}