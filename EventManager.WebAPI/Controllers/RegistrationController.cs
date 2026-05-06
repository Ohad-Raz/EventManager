using AutoMapper;
using EventManager.DAL.Models;
using EventManager.WebAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IMapper _mapper;
        // Database context, injected by DI
        private readonly EventManagerDbContext _context;

        public RegistrationController(EventManagerDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new event registration for the logged-in user.
        /// Prevents duplicate registration for the same event.
        /// Endpoint: POST /api/Registration
        /// </summary>
        [Authorize]
        [HttpPost]
        public ActionResult<RegistrationDto> Post(RegistrationDto registrationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 1. get logged-in username from JWT
                string? username = HttpContext.User.Identity?.Name;

                // 2. if identity is missing, return Unauthorized
                if (string.IsNullOrEmpty(username))
                    return Unauthorized("User identity is missing.");

                // 3. find logged-in user
                User? existingUser = _context.Users
                    .FirstOrDefault(x => x.Username == username);

                // 4. if user not found, return NotFound
                if (existingUser == null)
                    return NotFound("User not found.");

                // 5. find target event
                Event? existingEvent = _context.Events
                    .FirstOrDefault(e => e.Id == registrationDto.EventId);

                // 6. if event not found, return NotFound
                if (existingEvent == null)
                    return NotFound("Event not found.");

                // 7. check duplicate registration for same user and event
                Registration? existingRegistration = _context.Registrations
                    .FirstOrDefault(r => r.UserId == existingUser.Id && r.EventId == existingEvent.Id);

                // 8. if already registered, stop
                if (existingRegistration != null)
                    return BadRequest("User is already registered to the event.");

                // 9. create new registration entity
                Registration registration = new Registration
                {
                    Name = registrationDto.Name,
                    UserId = existingUser.Id,
                    EventId = existingEvent.Id,
                    IsActive = true
                };

                // 10. save to database
                _context.Registrations.Add(registration);
                _context.SaveChanges();

                // 11. return generated Id to client
                registrationDto.Id = registration.Id;

                return Ok(registrationDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Returns all registrations with related user and event data.
        /// Endpoint: GET /api/Registration
        /// Only Admins are allowed to access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<ICollection<RegistrationDetailsDto>> Get()
        {
            try
            {
                // 1. load registrations with related user and event
                List<Registration> registrations = _context.Registrations
                    .Include(x => x.User)
                    .Include(x => x.Event)
                    .ToList();

                // 2. map entities to DTOs
                List<RegistrationDetailsDto> result = _mapper.Map<List<RegistrationDetailsDto>>(registrations);

                // 3. return DTO list
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Returns one registration by id with related user and event data.
        /// Endpoint: GET /api/Registration/{id}
        /// Only Admins are allowed to access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public ActionResult<RegistrationDetailsDto> Get(int id)
        {
            try
            {
                // 1. load registration with related user and event
                Registration? registration = _context.Registrations
                    .Include(x => x.User)
                    .Include(x => x.Event)
                    .FirstOrDefault(x => x.Id == id);

                // 2. if not found, return NotFound
                if (registration == null)
                    return NotFound("Registration not found.");

                // 3. map entity to DTO
                RegistrationDetailsDto result = _mapper.Map<RegistrationDetailsDto>(registration);

                // 4. return tdo
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Returns registrations for the currently logged-in user.
        /// Includes related event data.
        /// Available to any authenticated user.
        /// Endpoint: GET /api/Registration/GetMyRegistrations
        /// </summary>
        [Authorize]
        [HttpGet("[action]")]
        public ActionResult<ICollection<RegistrationDetailsDto>> GetMyRegistrations()
        {
            try
            {
                // 1. get logged-in username from JWT
                string? username = HttpContext.User.Identity?.Name;

                // 2. if identity is missing, return Unauthorized
                if (string.IsNullOrEmpty(username))
                    return Unauthorized("User identity is missing.");

                // 3. load this user's registrations with related data
                List<Registration> registrations = _context.Registrations
                    .Include(x => x.User)
                    .Include(x => x.Event)
                    .Where(x => x.User.Username == username)
                    .ToList();

                // 4. map entities to DTOs
                List<RegistrationDetailsDto> result = _mapper.Map<List<RegistrationDetailsDto>>(registrations);

                // 5. return DTO list
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}