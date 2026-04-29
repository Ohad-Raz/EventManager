using AutoMapper;
using EventManager.WebAPI.Dtos;
using EventManager.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IMapper _mapper;
        // Database context, injected by DI
        private readonly EventManagerDbContext _context;

        public EventController(EventManagerDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
                // 1. load all events from database
                List<Event> events = _context.Events.ToList();

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
                // 1. find event by id
                Event? eventById = _context.Events.FirstOrDefault(e => e.Id == id);
                // 2. if not found, return NotFound
                if (eventById == null) return NotFound("Event not found.");
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

                // 2. start query from Events table
                IQueryable<Event> query = _context.Events.AsQueryable();

                // 3. if q has value, filter by Name or Description, case insensitive by default
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.Where(e =>
                        e.Name.Contains(q) ||
                        e.Description.Contains(q));
                }
                // 4. if eventTypeId has value, filter by EventTypeId
                if (eventTypeId.HasValue)
                {
                    query = query.Where(e => e.EventTypeId == eventTypeId.Value);
                }
                // 5. apply paging
                // Skip rows from previous pages, then take only the requested page size
                // Example: page=2 and count=10 means skip first 10 rows, then take next 10
                query = query
                    .Skip((page - 1) * count)
                    .Take(count);

                // 6. execute query and load matching events
                List<Event> events = query.ToList();

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
                if (eventDto.EndTime <= eventDto.StartTime)
                    return BadRequest("End time must be after start time.");
                // 2. get logged-in username from JWT
                string? username = HttpContext.User.Identity?.Name;

                // 3. if identity is missing, return Unauthorized
                if (string.IsNullOrEmpty(username))
                    return Unauthorized("User identity is missing.");

                // 4. find logged-in user
                User? existingUser = _context.Users.FirstOrDefault(x => x.Username == username);

                // 5. if user not found, return NotFound
                if (existingUser == null)
                    return NotFound("User not found.");

                // 6. validate referenced EventType
                EventType? existingEventType = _context.EventTypes
                    .FirstOrDefault(x => x.Id == eventDto.EventTypeId);

                if (existingEventType == null)
                    return NotFound("Event type not found.");
                // 7. validate optional Image
                if (eventDto.ImageId.HasValue)
                {
                    Models.Image? existingImage = _context.Images
                        .FirstOrDefault(x => x.Id == eventDto.ImageId.Value);

                    if (existingImage == null)
                        return NotFound("Image not found.");
                }
                // 8. create new Event entity
                Event newEvent = _mapper.Map<Event>(eventDto);
                newEvent.CreatedById = existingUser.Id;

                // 9. save to database
                _context.Events.Add(newEvent);
                _context.SaveChanges();

                // 10. copy generated Id back to DTO
                eventDto.Id = newEvent.Id;

                // 11. return created DTO
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
                if (eventDto.EndTime <= eventDto.StartTime)
                    return BadRequest("End time must be after start time.");
                // 2. find existing event by id
                Event? existingEvent = _context.Events.FirstOrDefault(e => e.Id == id);

                // 3. if not found, return NotFound
                if (existingEvent == null)
                    return NotFound($"Event with id={id} was not found.");

                // 4. validate referenced EventType and optional Image
                EventType? existingEventType = _context.EventTypes
                    .FirstOrDefault(x => x.Id == eventDto.EventTypeId);

                if (existingEventType == null)
                    return NotFound("Event type not found.");

                if (eventDto.ImageId.HasValue)
                {
                    Models.Image? existingImage = _context.Images
                        .FirstOrDefault(x => x.Id == eventDto.ImageId.Value);

                    if (existingImage == null)
                        return NotFound("Image not found.");
                }

                // 5. update editable fields
                _mapper.Map(eventDto, existingEvent);

                // 6. save changes
                _context.SaveChanges();

                // 7. return updated DTO
                eventDto.Id = existingEvent.Id;
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
                // 1. find existing event by id
                Event? existingEvent = _context.Events.FirstOrDefault(e => e.Id == id);
                // 2. if not found, return NotFound
                if (existingEvent == null)
                    return NotFound($"Event with id={id} was not found.");
                // 3. remove event from database
                _context.Events.Remove(existingEvent);//can throw exception if referenced as fk!
                // 4. save changes
                _context.SaveChanges();
                // 5. return success response
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
                // 1. find event by id
                Event? eventById = _context.Events.FirstOrDefault(e => e.Id == id);

                // 2. if event not found, return NotFound
                if (eventById == null)
                    return NotFound("Event not found.");

                // 3. load performer relations for that event
                List<EventPerformer> eventPerformers = _context.EventPerformers
                    .Include(x => x.Performer)
                    .Where(x => x.EventId == id)
                    .ToList();

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
                // 2. find event by id
                Event? eventById = _context.Events.FirstOrDefault(e => e.Id == id);

                // 3. if event not found, return NotFound
                if (eventById == null)
                    return NotFound("Event not found.");
                // 4. find performer by id from DTO
                Performer? performer = _context.Performers
                .FirstOrDefault(p => p.Id == eventPerformerDto.PerformerId);
                // 5. if performer not found, return NotFound
                if (performer == null) return NotFound($"Performer with id={eventPerformerDto.PerformerId} was not found.");

                // 6. check whether relation already exists
                bool existingRelation = _context.EventPerformers
                    .Any(ep => ep.EventId == id && ep.PerformerId == eventPerformerDto.PerformerId);
                // EventPerformer? existingRelation = _context.EventPerformers
                //.FirstOrDefault(ep => ep.EventId == id && ep.PerformerId == eventPerformerDto.PerformerId);
                // 7. if relation already exists, return BadRequest
                if (existingRelation)
                    return BadRequest($"Event id={id} already contains performer id={eventPerformerDto.PerformerId}.");

                // 8. create new EventPerformer entity
                EventPerformer PatchedPerformer = new EventPerformer
                {
                    EventId = id,
                    PerformerId = eventPerformerDto.PerformerId
                };

                //eventById.EventPerformers.Add(PatchedPerformer);
                _context.EventPerformers.Add(PatchedPerformer);

                // 9. save to database
                _context.SaveChanges();
                // 10. return success response
                return Ok($"Performer id={PatchedPerformer.PerformerId} was added to Event id={eventById.Id}.");
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
                // 1. find event by id
                Event? eventById = _context.Events.FirstOrDefault(e => e.Id == id);


                // 2. if event not found, return NotFound
                if (eventById == null)
                    return NotFound("Event not found.");
                // 3. find performer by id
                Performer? performer = _context.Performers
            .FirstOrDefault(p => p.Id == performerId);
                // 4. if performer not found, return NotFound
                if (performer == null) return NotFound($"Performer with id={performerId} was not found.");

                // 5. find relation row in EventPerformer
                EventPerformer? existingRelation = _context.EventPerformers
                    .FirstOrDefault(ep => ep.EventId == id && ep.PerformerId == performerId);
                // 6. if relation not found, return NotFound
                if (existingRelation == null)
                    return NotFound($"Performer id={performerId} is not assigned to event id={id}.");
                // 7. remove relation from database
                _context.EventPerformers.Remove(existingRelation);
                // 8. save changes
                _context.SaveChanges();
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