using AutoMapper;
using EventManager.DAL.Models;
using EventManager.DAL.Repositories;
using EventManager.WebAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EventManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IRegistrationRepository _registrationRepository;

        public RegistrationController(IRegistrationRepository registrationRepository, IMapper mapper)
        {
            _registrationRepository = registrationRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new event registration for the logged-in user (User role only).
        /// Prevents duplicate registration for the same event.
        /// Endpoint: POST /api/Registration
        /// </summary>
        [Authorize(Roles = "User")]
        [HttpPost]
        public ActionResult<RegistrationDto> Post(RegistrationDto registrationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // The JWT stores the database user id as the raw nameid claim.
                string? userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.NameId);

                if (!int.TryParse(userIdValue, out int userId))
                    return Unauthorized("User id claim is missing or invalid.");

                // 1. find target event
                Event? existingEvent = _registrationRepository.GetEventById(registrationDto.EventId);

                // 2. if event not found, return NotFound
                if (existingEvent == null)
                    return NotFound("Event not found.");

                // 3. check existing registration for same user and event
                Registration? existingRegistration =
                    _registrationRepository.GetRegistrationByUserAndEvent(userId, existingEvent.Id);

                // 4. if active registration already exists, stop
                if (existingRegistration != null && existingRegistration.IsActive)
                    return BadRequest("User is already registered to the event.");

                // 5. if cancelled registration exists, reactivate it
                if (existingRegistration != null && !existingRegistration.IsActive)
                {
                    existingRegistration.IsActive = true;
                    existingRegistration.Name = registrationDto.Name;

                    _registrationRepository.SaveChanges();

                    registrationDto.Id = existingRegistration.Id;

                    return Ok(registrationDto);
                }
                // 6. create new registration entity
                Registration registration = new Registration
                {
                    Name = registrationDto.Name,
                    UserId = userId,
                    EventId = existingEvent.Id,
                    IsActive = true
                };
                // 7. save to database
                _registrationRepository.AddRegistration(registration);
                _registrationRepository.SaveChanges();

                // 8. return generated Id to client
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
                List<Registration> registrations = _registrationRepository.GetAllRegistrationsWithDetails();

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
        //Only match this action if the URL value is an integer.
        [HttpGet("{id:int}")]
        public ActionResult<RegistrationDetailsDto> Get([FromRoute] int id)
        {
            try
            {
                // 1. load registration with related user and event
                Registration? registration = _registrationRepository.GetRegistrationByIdWithDetails(id);

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
        /// Only Users can view their own registrations.
        /// Endpoint: GET /api/Registration/GetMyRegistrations
        /// </summary>
        [Authorize(Roles = "User")]
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
                List<Registration> registrations = _registrationRepository.GetRegistrationsByUsernameWithDetails(username);

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