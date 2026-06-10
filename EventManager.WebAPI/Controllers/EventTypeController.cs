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
    public class EventTypeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IEventTypeRepository _eventTypeRepository;

        public EventTypeController(IEventTypeRepository eventTypeRepository, IMapper mapper)
        {
            _eventTypeRepository = eventTypeRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all event types.
        /// Endpoint: GET /api/EventType
        /// </summary>
        [HttpGet]
        public ActionResult<ICollection<EventTypeDto>> Get()
        {
            try
            {
                List<EventType> eventTypes = _eventTypeRepository.GetAllEventTypes();
                List<EventTypeDto> result = _mapper.Map<List<EventTypeDto>>(eventTypes);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Returns one event type by id.
        /// Endpoint: GET /api/EventType/{id}
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<EventTypeDto> Get(int id)
        {
            try
            {
                EventType? eventType = _eventTypeRepository.GetEventTypeById(id);
                if (eventType == null)
                    return NotFound($"EventType with id={id} was not found.");

                EventTypeDto result = _mapper.Map<EventTypeDto>(eventType);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Creates a new event type.
        /// Endpoint: POST /api/EventType
        /// </summary>
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public ActionResult<EventTypeDto> Post(EventTypeDto eventTypeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string trimmedName = eventTypeDto.Name.Trim();
                if (_eventTypeRepository.EventTypeNameExists(trimmedName))
                    return BadRequest($"EventType name {trimmedName} already exists.");

                EventType newEventType = _mapper.Map<EventType>(eventTypeDto);
                newEventType.Name = trimmedName;

                _eventTypeRepository.AddEventType(newEventType);
                _eventTypeRepository.SaveChanges();

                eventTypeDto.Id = newEventType.Id;
                eventTypeDto.Name = newEventType.Name;
                return Ok(eventTypeDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing event type.
        /// Endpoint: PUT /api/EventType/{id}
        /// </summary>
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id}")]
        public ActionResult<EventTypeDto> Put(int id, EventTypeDto eventTypeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                EventType? existingEventType = _eventTypeRepository.GetEventTypeById(id);
                if (existingEventType == null)
                    return NotFound($"EventType with id={id} was not found.");

                string trimmedName = eventTypeDto.Name.Trim();
                if (_eventTypeRepository.EventTypeNameExists(trimmedName, id))
                    return BadRequest($"EventType name {trimmedName} already exists.");

                existingEventType.Name = trimmedName;
                _eventTypeRepository.SaveChanges();

                eventTypeDto.Id = existingEventType.Id;
                eventTypeDto.Name = existingEventType.Name;
                return Ok(eventTypeDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Deletes an event type by id.
        /// Endpoint: DELETE /api/EventType/{id}
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                EventType? existingEventType = _eventTypeRepository.GetEventTypeById(id);
                if (existingEventType == null)
                    return NotFound($"EventType with id={id} was not found.");

                if (_eventTypeRepository.EventTypeHasEvents(id))
                    return BadRequest($"EventType with id={id} cannot be deleted because events use it.");

                _eventTypeRepository.RemoveEventType(existingEventType);
                _eventTypeRepository.SaveChanges();
                return Ok($"EventType with id={id} has been deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
