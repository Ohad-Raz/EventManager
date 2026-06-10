using EventManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.DAL.Repositories
{
    public class DbEventPerformerRepository : IEventPerformerRepository
    {
        private readonly EventManagerDbContext _context;

        public DbEventPerformerRepository(EventManagerDbContext context)
        {
            _context = context;
        }

        public List<EventPerformer> GetEventPerformersByEventId(int eventId)
        {
            return _context.EventPerformers
                .Include(x => x.Performer)
                .Where(x => x.EventId == eventId)
                .ToList();
        }

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

        public bool EventPerformerRelationExists(int eventId, int performerId)
        {
            return _context.EventPerformers
                .Any(ep => ep.EventId == eventId && ep.PerformerId == performerId);
        }

        public EventPerformer? GetEventPerformerRelation(int eventId, int performerId)
        {
            return _context.EventPerformers
                .FirstOrDefault(ep => ep.EventId == eventId && ep.PerformerId == performerId);
        }

        public void AddEventPerformer(EventPerformer eventPerformer)
        {
            _context.EventPerformers.Add(eventPerformer);
        }

        public void RemoveEventPerformer(EventPerformer existingRelation)
        {
            _context.EventPerformers.Remove(existingRelation);
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }
    }
}
