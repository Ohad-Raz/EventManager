using EventManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.DAL.Repositories
{
    public class DbEventRepository : IEventRepository
    {
        private readonly EventManagerDbContext _context;

        public DbEventRepository(EventManagerDbContext context)
        {
            _context = context;
        }

        // Returns one event by id.
        public Event? GetEventById(int id)
        {
            // 1. find event by id only if not soft-deleted
            return _context.Events.FirstOrDefault(e => e.Id == id && e.DeletedAt == null);
        }

        // Returns one event by id with related entities.
        public Event? GetEventByIdWithDetails(int id)
        {
            // 1. find event by id with all details needed by MVC Details page
            return _context.Events
                .Include(e => e.CreatedBy)
                .Include(e => e.EventType)
                .Include(e => e.Image)
                // Loads performer data together with the selected event for the Details page.
                .Include(e => e.EventPerformers)
                    .ThenInclude(ep => ep.Performer)
                .FirstOrDefault(e => e.Id == id && e.DeletedAt == null);
        }

        // Returns all events.
        public List<Event> GetAllEvents()
        {
            // 1. load all non-deleted events from database
            return _context.Events.Where(e => e.DeletedAt == null).ToList();
        }

        // Returns all events with related entities.
        public List<Event> GetAllEventsWithDetails()
        {
            // 1. load all non-deleted events with related entities
            return _context.Events
                .Include(e => e.CreatedBy)
                .Include(e => e.EventType)
                .Include(e => e.Image)
                .Include(e => e.EventPerformers)
                    .ThenInclude(ep => ep.Performer)
                .Where(e => e.DeletedAt == null)
                .ToList();
        }

        // Returns paged events filtered by optional query and event type.
        public List<Event> SearchEvents(string? q, int? eventTypeId, int page, int count)
        {
            // 1. start query from non-deleted Events
            IQueryable<Event> query = _context.Events.Where(e => e.DeletedAt == null);

            // 2. if q has value, filter by Name or Description
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(e =>
                    e.Name.Contains(q) ||
                    e.Description.Contains(q));
            }

            // 3. if eventTypeId has value, filter by EventTypeId
            if (eventTypeId.HasValue)
            {
                query = query.Where(e => e.EventTypeId == eventTypeId.Value);
            }

            // 4. apply paging with Skip/Take
            query = query
                .Skip((page - 1) * count)
                .Take(count);

            // 5. execute query
            return query.ToList();
        }

        // Checks whether event exists and is not soft-deleted.
        public bool EventExists(int id)
        {
            // 1. check event existence by id for active events
            return _context.Events.Any(e => e.Id == id && e.DeletedAt == null);
        }

        // Returns one user by username.
        public User? GetUserByUsername(string username)
        {
            // 1. check if user exists by username
            return _context.Users.FirstOrDefault(x => x.Username == username);
        }

        // Returns one event type by id.
        public EventType? GetEventTypeById(int id)
        {
            // 1. check if EventType exists
            return _context.EventTypes.FirstOrDefault(x => x.Id == id);
        }

        // Returns one image by id.
        public Image? GetImageById(int id)
        {
            // 1. check if Image exists
            return _context.Images.FirstOrDefault(x => x.Id == id);
        }

        // Returns all users for dropdowns.
        public List<User> GetAllUsers()
        {
            // 1. load all users from database
            return _context.Users.ToList();
        }

        // Returns all event types for dropdowns.
        public List<EventType> GetAllEventTypes()
        {
            // 1. load all event types from database
            return _context.EventTypes.ToList();
        }

        // Returns all images for dropdowns.
        public List<Image> GetAllImages()
        {
            // 1. load all images from database
            return _context.Images.ToList();
        }

        // Adds a new event type row.
        public void AddEventType(EventType eventType)
        {
            _context.EventTypes.Add(eventType);
        }

        // Marks an event type row as modified.
        public void UpdateEventType(EventType eventType)
        {
            _context.EventTypes.Update(eventType);
        }

        // Removes an event type row.
        public void RemoveEventType(EventType eventType)
        {
            _context.EventTypes.Remove(eventType);
        }

        // Checks whether an event type name already exists.
        public bool EventTypeNameExists(string name, int? excludeId = null)
        {
            string trimmedName = name.Trim();

            if (excludeId.HasValue)
            {
                // Edit: allow the current row to keep its own name.
                return _context.EventTypes.Any(x =>
                    x.Name == trimmedName && x.Id != excludeId.Value);
            }

            // Create: any row with this name is a duplicate.
            return _context.EventTypes.Any(x => x.Name == trimmedName);
        }

        // Checks whether any events reference this event type.
        public bool EventTypeHasEvents(int eventTypeId)
        {
            return _context.Events.Any(e => e.EventTypeId == eventTypeId && e.DeletedAt == null);
        }

        // Adds a new event row.
        public void AddEvent(Event newEvent)
        {
            // 1. add Event row
            _context.Events.Add(newEvent);
        }

        // Marks an event row as modified.
        public void UpdateEvent(Event existingEvent)
        {
            // 1. update Event row
            _context.Events.Update(existingEvent);
        }

        // Soft-deletes an event and deactivates its active registrations.
        public void RemoveEvent(Event existingEvent)
        {
            // 1. perform soft delete by setting DeletedAt
            existingEvent.DeletedAt = DateTime.UtcNow;

            // 2. load all active registrations for this event
            List<Registration> activeRegistrations = _context.Registrations
                .Where(r => r.EventId == existingEvent.Id && r.IsActive)
                .ToList();

            // 3. mark related registrations as inactive
            foreach (Registration registration in activeRegistrations)
            {
                registration.IsActive = false;
            }
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        // Returns all event-performer links for one event.
        public List<EventPerformer> GetEventPerformersByEventId(int eventId)
        {
            // 1. load performer relations for this event
            return _context.EventPerformers
                .Include(x => x.Performer)
                .Where(x => x.EventId == eventId)
                .ToList();
        }

        // Returns performers not yet linked to the given event.
        public List<Performer> GetUnassignedPerformersForEvent(int eventId)
        {
            List<int> assignedPerformerIds = _context.EventPerformers
                .Where(ep => ep.EventId == eventId)
                .Select(ep => ep.PerformerId)
                .ToList();

            return _context.Performers
                .Where(p => !assignedPerformerIds.Contains(p.Id))
                .OrderBy(p => p.Name)
                .ToList();
        }

        // Returns one performer by id.
        public Performer? GetPerformerById(int performerId)
        {
            // 1. check if Performer exists
            return _context.Performers.FirstOrDefault(p => p.Id == performerId);
        }

        // Checks whether an event-performer link exists.
        public bool EventPerformerRelationExists(int eventId, int performerId)
        {
            // 1. check whether relation already exists
            return _context.EventPerformers
                .Any(ep => ep.EventId == eventId && ep.PerformerId == performerId);
        }

        // Returns one event-performer link by event and performer ids.
        public EventPerformer? GetEventPerformerRelation(int eventId, int performerId)
        {
            // 1. find relation row in EventPerformer
            return _context.EventPerformers
                .FirstOrDefault(ep => ep.EventId == eventId && ep.PerformerId == performerId);
        }

        // Adds an event-performer link row.
        public void AddEventPerformer(EventPerformer eventPerformer)
        {
            // 1. add EventPerformer row
            _context.EventPerformers.Add(eventPerformer);
        }

        // Removes an event-performer link row.
        public void RemoveEventPerformer(EventPerformer existingRelation)
        {
            // 1. remove EventPerformer row
            _context.EventPerformers.Remove(existingRelation);
        }
    }
}
