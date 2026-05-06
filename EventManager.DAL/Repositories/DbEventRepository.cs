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
            // 1. find event by id
            return _context.Events.FirstOrDefault(e => e.Id == id);
        }

        // Returns all events.
        public List<Event> GetAllEvents()
        {
            // 1. load all events from database
            return _context.Events.ToList();
        }

        // Returns paged events filtered by optional query and event type.
        public List<Event> SearchEvents(string? q, int? eventTypeId, int page, int count)
        {
            // 1. start query from Events table
            IQueryable<Event> query = _context.Events.AsQueryable();

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

        // Adds a new event row.
        public void AddEvent(Event newEvent)
        {
            // 1. add Event row
            _context.Events.Add(newEvent);
        }

        // Removes an event row.
        public void RemoveEvent(Event existingEvent)
        {
            // 1. remove Event row
            _context.Events.Remove(existingEvent);
        }

        // Saves pending database changes.
        public int SaveChanges()
        {
            // 1. call SaveChanges
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
