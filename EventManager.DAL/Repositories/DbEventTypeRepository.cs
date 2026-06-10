using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public class DbEventTypeRepository : IEventTypeRepository
    {
        private readonly EventManagerDbContext _context;

        public DbEventTypeRepository(EventManagerDbContext context)
        {
            _context = context;
        }

        public EventType? GetEventTypeById(int id)
        {
            return _context.EventTypes.FirstOrDefault(x => x.Id == id);
        }

        public List<EventType> GetAllEventTypes()
        {
            return _context.EventTypes.ToList();
        }

        public void AddEventType(EventType eventType)
        {
            _context.EventTypes.Add(eventType);
        }

        public void UpdateEventType(EventType eventType)
        {
            _context.EventTypes.Update(eventType);
        }

        public void RemoveEventType(EventType eventType)
        {
            _context.EventTypes.Remove(eventType);
        }

        public bool EventTypeNameExists(string name, int? excludeId = null)
        {
            string trimmedName = name.Trim();

            if (excludeId.HasValue)
            {
                return _context.EventTypes.Any(x =>
                    x.Name == trimmedName && x.Id != excludeId.Value);
            }

            return _context.EventTypes.Any(x => x.Name == trimmedName);
        }

        public bool EventTypeHasEvents(int eventTypeId)
        {
            return _context.Events.Any(e => e.EventTypeId == eventTypeId && e.DeletedAt == null);
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }
    }
}
