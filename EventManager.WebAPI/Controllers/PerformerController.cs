using EventManager.WebAPI.Dtos;
using EventManager.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PerformerController : ControllerBase
    {
        // Database context, injected by DI
        private readonly EventManagerDbContext _context;

        public PerformerController(EventManagerDbContext context)
        {
            _context = context;
        }
        private static PerformerDto MapToDto(Performer performer)
        {
            return new PerformerDto
            {
                Id = performer.Id,
                Name = performer.Name,
                Bio = performer.Bio
            };
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
                List<Performer> performers = _context.Performers.ToList();
                // 2. map entities to DTOs
                List<PerformerDto> result = performers.Select(p => MapToDto(p)).ToList();
                // 3. return DTO list
                return Ok(result);
                //auto mapper
                //
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Returns one performer by id.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<PerformerDto> Get(int id)
        {
            try
            {
                // 1. find performer by id
                Performer? performer = _context.Performers.FirstOrDefault(p => p.Id == id);

                // 2. if not found, return NotFound
                if (performer == null) return NotFound($"Performer with id={id} was not found.");
                // 3. map entity to DTO
                PerformerDto result = MapToDto(performer);
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
                Performer newPerformer = new Performer
                {
                    Name = performerDto.Name,
                    Bio = performerDto.Bio,
                };
                // 3. save to database
                _context.Performers.Add(newPerformer);
                _context.SaveChanges();
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
                Performer? performer = _context.Performers.FirstOrDefault(p => p.Id == id);
                // 3. if not found, return NotFound
                if (performer == null) return NotFound($"Performer with id={id} was not found.");
                // 4. update editable fields
                performer.Name = performerDto.Name;
                performer.Bio = performerDto.Bio;
                // 5. save changes
                _context.SaveChanges();
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
        /// Only Admins are allowed to access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                // 1. find existing performer by id
                Performer? performer = _context.Performers.FirstOrDefault(p => p.Id == id);
                // 2. if not found, return NotFound
                if (performer == null) return NotFound($"Performer with id={id} was not found.");
                // 3. remove performer from database
                _context.Performers.Remove(performer);
                // 4. save changes
                _context.SaveChanges();
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