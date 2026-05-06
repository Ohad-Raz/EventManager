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

        // Removes a performer row.
        public void RemovePerformer(Performer performer)
        {
            // 1. remove Performer row
            _context.Performers.Remove(performer);
        }

        // Saves pending database changes.
        public int SaveChanges()
        {
            // 1. call SaveChanges
            return _context.SaveChanges();
        }
    }
}
