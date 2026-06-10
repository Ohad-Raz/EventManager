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

        public Event? GetEventById(int id)
        {
            return _context.Events.FirstOrDefault(e => e.Id == id && e.DeletedAt == null);
        }

        public Event? GetEventByIdWithDetails(int id)
        {
            return _context.Events
                .Include(e => e.CreatedBy)
                .Include(e => e.EventType)
                .Include(e => e.Image)
                .Include(e => e.EventPerformers)
                    .ThenInclude(ep => ep.Performer)
                .FirstOrDefault(e => e.Id == id && e.DeletedAt == null);
        }

        public List<Event> GetAllEvents()
        {
            return _context.Events.Where(e => e.DeletedAt == null).ToList();
        }

        public List<Event> GetAllEventsWithDetails()
        {
            return _context.Events
                .Include(e => e.CreatedBy)
                .Include(e => e.EventType)
                .Include(e => e.Image)
                .Include(e => e.EventPerformers)
                    .ThenInclude(ep => ep.Performer)
                .Where(e => e.DeletedAt == null)
                .ToList();
        }

        public List<Event> SearchEvents(string? q, int? eventTypeId, int page, int count)
        {
            IQueryable<Event> query = _context.Events.Where(e => e.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(e =>
                    e.Name.Contains(q) ||
                    e.Description.Contains(q));
            }

            if (eventTypeId.HasValue)
            {
                query = query.Where(e => e.EventTypeId == eventTypeId.Value);
            }

            query = query
                .Skip((page - 1) * count)
                .Take(count);

            return query.ToList();
        }

        public bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id && e.DeletedAt == null);
        }

        public Image? GetImageById(int id)
        {
            return _context.Images.FirstOrDefault(x => x.Id == id);
        }

        public List<Image> GetAllImages()
        {
            return _context.Images.ToList();
        }

        public void AddEvent(Event newEvent)
        {
            _context.Events.Add(newEvent);
        }

        public void UpdateEvent(Event existingEvent)
        {
            _context.Events.Update(existingEvent);
        }

        public void RemoveEvent(Event existingEvent)
        {
            existingEvent.DeletedAt = DateTime.UtcNow;

            List<Registration> activeRegistrations = _context.Registrations
                .Where(r => r.EventId == existingEvent.Id && r.IsActive)
                .ToList();

            foreach (Registration registration in activeRegistrations)
            {
                registration.IsActive = false;
            }
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }
    }
}
