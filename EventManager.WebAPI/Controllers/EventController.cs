using AutoMapper;
using EventManager.DAL.Models;
using EventManager.DAL.Repositories;
using EventManager.WebAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EventManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IEventRepository _eventRepository;
        private readonly IEventTypeRepository _eventTypeRepository;
        private readonly IEventPerformerRepository _eventPerformerRepository;
        private readonly IPerformerRepository _performerRepository;
        private readonly ILogRepository _logRepository;

        public EventController(
            IEventRepository eventRepository,
            IEventTypeRepository eventTypeRepository,
            IEventPerformerRepository eventPerformerRepository,
            IPerformerRepository performerRepository,
            ILogRepository logRepository,
            IMapper mapper)
        {
            _eventRepository = eventRepository;
            _eventTypeRepository = eventTypeRepository;
            _eventPerformerRepository = eventPerformerRepository;
            _performerRepository = performerRepository;
            _logRepository = logRepository;
            _mapper = mapper;
        }

        // Writes a log row to the database.
        // Logging should never break the main action flow,
        // so failures inside this helper are swallowed.
        private void WriteLog(int level, string message, string? errorText = null)
        {
            try
            {
                Log log = new Log
                {
                    Level = level,
                    Message = message,
                    ErrorText = errorText
                };
                _logRepository.AddLog(log);
                _logRepository.SaveChanges();
            }
            catch
            {
                // Ignore logging failures so they do not break the main request.
            }
        }

        /// <summary>
        /// Returns all events with related event type and image data.
        /// Endpoint: GET /api/Event
        /// </summary>
        [HttpGet]
        public ActionResult<ICollection<EventDto>> Get()
        {
            try
            {
                // 1. load data for this request
                List<Event> events = _eventRepository.GetAllEvents();

                // 2. map entities to DTOs
                List<EventDto> result = _mapper.Map<List<EventDto>>(events);

                // 3. return DTO list
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Returns one event by id.
        /// Endpoint: GET /api/Event/{id}
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<EventDto> Get(int id)
        {
            try
            {
                // 1. load data for this request
                Event? eventById = _eventRepository.GetEventById(id);

                // 2. if not found, return NotFound
                if (eventById == null)
                    return NotFound("Event not found.");

                // 3. map entity to DTO
                EventDto result = _mapper.Map<EventDto>(eventById);

                // 4. return DTO
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Searches events by text, optional event type, and paging.
        /// Endpoint: GET /api/Event/Search?q={text}&eventTypeId={id}&page={page}&count={count}
        /// </summary>
        [HttpGet("[action]")]
        public ActionResult<ICollection<EventDto>> Search(string? q, int? eventTypeId, int page = 1, int count = 10)
        {
            try
            {
                // 1. validate paging input
                if (page < 1)
                    return BadRequest("Page must be at least 1.");
                if (count < 1)
                    return BadRequest("Count must be at least 1.");

                // 2. load data for this request
                List<Event> events = _eventRepository.SearchEvents(q, eventTypeId, page, count);

                // 7. map entities to DTOs
                List<EventDto> result = _mapper.Map<List<EventDto>>(events);

                // 8. return DTO list
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        /// <summary>
        /// Creates a new event.
        /// Endpoint: POST /api/Event
        /// Only Admin and Organizer are allowed to access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public ActionResult<EventDto> Post(EventDto eventDto)
        {
            try
            {
                // 1. validate model state
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 2. read user id from JWT nameid claim
                string? userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.NameId);

                // 3. if claim is missing or invalid, return Unauthorized
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("User id claim is missing.");

                // 4. validate referenced EventType
                EventType? existingEventType = _eventTypeRepository.GetEventTypeById(eventDto.EventTypeId);

                if (existingEventType == null)
                {
                    WriteLog(4, $"Cannot find EventType id={eventDto.EventTypeId} while creating event.");
                    return NotFound("Event type not found.");
                }

                // 5. validate optional Image
                if (eventDto.ImageId.HasValue)
                {
                    Image? existingImage = _eventRepository.GetImageById(eventDto.ImageId.Value);

                    if (existingImage == null)
                    {
                        WriteLog(4, $"Cannot find Image id={eventDto.ImageId.Value} while creating event.");
                        return NotFound("Image not found.");
                    }
                }

                // 6. create new Event entity
                Event newEvent = _mapper.Map<Event>(eventDto);
                newEvent.CreatedById = userId;

                // 9. save changes
                _eventRepository.AddEvent(newEvent);
                _eventRepository.SaveChanges();

                // 10. copy generated Id back to DTO
                eventDto.Id = newEvent.Id;

                // 11. return created DTO
                WriteLog(2, $"Event with id={newEvent.Id} has been created.");
                return Ok(eventDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing event.
        /// Endpoint: PUT /api/Event/{id}
        /// Only Admin and Organizer are allowed to access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id}")]
        public ActionResult<EventDto> Put(int id, EventDto eventDto)
        {
            try
            {
                // 1. validate model state
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 2. load data for this request
                Event? existingEvent = _eventRepository.GetEventById(id);

                // 3. if not found, return NotFound
                if (existingEvent == null)
                {
                    WriteLog(4, $"Cannot find Event id={id} for update.");
                    return NotFound($"Event with id={id} was not found.");
                }

                // 4. validate referenced EventType and optional Image
                EventType? existingEventType = _eventTypeRepository.GetEventTypeById(eventDto.EventTypeId);

                if (existingEventType == null)
                {
                    WriteLog(4, $"Cannot find EventType id={eventDto.EventTypeId} while updating Event id={id}.");
                    return NotFound("Event type not found.");
                }

                if (eventDto.ImageId.HasValue)
                {
                    Image? existingImage = _eventRepository.GetImageById(eventDto.ImageId.Value);

                    if (existingImage == null)
                    {
                        WriteLog(4, $"Cannot find Image id={eventDto.ImageId.Value} while updating Event id={id}.");
                        return NotFound("Image not found.");
                    }
                }

                // 5. update editable fields
                _mapper.Map(eventDto, existingEvent);

                // 6. save changes
                _eventRepository.SaveChanges();

                // 7. return updated DTO
                eventDto.Id = existingEvent.Id;
                WriteLog(2, $"Event with id={existingEvent.Id} has been updated.");
                return Ok(eventDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Deletes an event by id.
        /// Endpoint: DELETE /api/Event/{id}
        /// Only Admin and Organizer are allowed to access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin,Organizer")]
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                // 1. load data for this request
                Event? existingEvent = _eventRepository.GetEventById(id);

                // 2. if not found, return NotFound
                if (existingEvent == null)
                {
                    WriteLog(4, $"Cannot find Event id={id} for delete.");
                    return NotFound($"Event with id={id} was not found.");
                }

                // 3. remove event from database
                _eventRepository.RemoveEvent(existingEvent);

                // 4. save changes
                _eventRepository.SaveChanges();

                // 5. return success response
                WriteLog(2, $"Event with id={id} has been deleted.");
                return Ok($"Event with id={id} was deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Returns all performers assigned to a given event.
        /// Endpoint: GET /api/Event/{id}/Performers
        /// </summary>
        [HttpGet("{id}/Performers")]
        public ActionResult<ICollection<PerformerDto>> GetPerformers(int id)
        {
            try
            {
                // 1. load data for this request
                Event? eventById = _eventRepository.GetEventById(id);

                // 2. if event not found, return NotFound
                if (eventById == null)
                    return NotFound("Event not found.");

                // 3. load data for this request
                List<EventPerformer> eventPerformers = _eventPerformerRepository.GetEventPerformersByEventId(id);

                // 4. map related performers to DTO list
                List<PerformerDto> result = eventPerformers
                    .Select(x => new PerformerDto
                    {
                        Id = x.Performer.Id,
                        Name = x.Performer.Name,
                        Bio = x.Performer.Bio
                    })
                    .ToList();

                // 5. return DTO list
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Adds a performer to a given event.
        /// Creates a new EventPerformer relation.
        /// Endpoint: PATCH /api/Event/{id}/Performers
        /// Only Admin and Organizer are allowed to access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPatch("{id}/Performers")]
        public ActionResult PatchPerformers(int id, EventPerformerDto eventPerformerDto)
        {
            try
            {
                // 1. validate model state
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 2. load data for this request
                Event? eventById = _eventRepository.GetEventById(id);

                // 3. if event not found, return NotFound
                if (eventById == null)
                    return NotFound("Event not found.");

                // 4. load data for this request
                Performer? performer = _performerRepository.GetPerformerById(eventPerformerDto.PerformerId);

                // 5. if performer not found, return NotFound
                if (performer == null)
                    return NotFound($"Performer with id={eventPerformerDto.PerformerId} was not found.");

                // 6. check whether relation already exists
                bool existingRelation = _eventPerformerRepository.EventPerformerRelationExists(id, eventPerformerDto.PerformerId);

                // 7. if relation already exists, return BadRequest
                if (existingRelation)
                    return BadRequest($"Event id={id} already contains performer id={eventPerformerDto.PerformerId}.");

                // 8. create new EventPerformer entity
                EventPerformer patchedPerformer = new EventPerformer
                {
                    EventId = id,
                    PerformerId = eventPerformerDto.PerformerId
                };

                _eventPerformerRepository.AddEventPerformer(patchedPerformer);

                // 9. save changes
                _eventPerformerRepository.SaveChanges();

                // 10. return success response
                return Ok($"Performer id={patchedPerformer.PerformerId} was added to Event id={eventById.Id}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Removes a performer from a given event.
        /// Deletes the related EventPerformer row.
        /// Endpoint: DELETE /api/Event/{id}/Performers/{performerId}
        /// Only Admin and Organizer are allowed to access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin,Organizer")]
        [HttpDelete("{id}/Performers/{performerId}")]
        public ActionResult DeletePerformer(int id, int performerId)
        {
            try
            {
                // 1. load data for this request
                Event? eventById = _eventRepository.GetEventById(id);

                // 2. if event not found, return NotFound
                if (eventById == null)
                    return NotFound("Event not found.");

                // 3. load data for this request
                Performer? performer = _performerRepository.GetPerformerById(performerId);

                // 4. if performer not found, return NotFound
                if (performer == null)
                    return NotFound($"Performer with id={performerId} was not found.");

                // 5. load data for this request
                EventPerformer? existingRelation = _eventPerformerRepository.GetEventPerformerRelation(id, performerId);

                // 6. if relation not found, return NotFound
                if (existingRelation == null)
                    return NotFound($"Performer id={performerId} is not assigned to event id={id}.");

                // 7. remove relation
                _eventPerformerRepository.RemoveEventPerformer(existingRelation);

                // 8. save changes
                _eventPerformerRepository.SaveChanges();

                // 9. return success response
                return Ok($"Performer id={performerId} was deleted from Event id={eventById.Id}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}