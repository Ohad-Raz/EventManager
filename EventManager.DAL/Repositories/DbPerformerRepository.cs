using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public class DbPerformerRepository : IPerformerRepository
    {
        private readonly EventManagerDbContext _context;

        public DbPerformerRepository(EventManagerDbContext context)
        {
            _context = context;
        }

        // Returns all performers.
        public List<Performer> GetAllPerformers()
        {
            // 1. load all performers from database
            return _context.Performers.ToList();
        }

        // Returns one performer by id.
        public Performer? GetPerformerById(int id)
        {
            // 1. find performer by id
            return _context.Performers.FirstOrDefault(p => p.Id == id);
        }

        // Adds a new performer row.
        public void AddPerformer(Performer performer)
        {
            // 1. add Performer row
            _context.Performers.Add(performer);
        }

        // Marks a performer row as modified.
        public void UpdatePerformer(Performer performer)
        {
            _context.Performers.Update(performer);
        }

        // Removes a performer row.
        public void RemovePerformer(Performer performer)
        {
            // 1. remove Performer row
            _context.Performers.Remove(performer);
        }

        // Checks whether a performer name already exists.
        public bool PerformerNameExists(string name, int? excludeId = null)
        {
            string trimmedName = name.Trim();

            if (excludeId.HasValue)
            {
                // Edit: allow the current row to keep its own name.
                return _context.Performers.Any(x =>
                    x.Name == trimmedName && x.Id != excludeId.Value);
            }

            // Create: any row with this name is a duplicate.
            return _context.Performers.Any(x => x.Name == trimmedName);
        }

        // Checks whether the performer is assigned to any event.
        public bool PerformerHasEventAssignments(int performerId)
        {
            return _context.EventPerformers.Any(ep => ep.PerformerId == performerId);
        }

        // Saves pending database changes.
        public int SaveChanges()
        {
            // 1. call SaveChanges
            return _context.SaveChanges();
        }
    }
}
