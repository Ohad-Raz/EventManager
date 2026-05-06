using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public class DbLogRepository : ILogRepository
    {
        private readonly EventManagerDbContext _context;

        public DbLogRepository(EventManagerDbContext context)
        {
            _context = context;
        }

        // Returns total number of logs.
        public int CountLogs()
        {
            // 1. count all logs
            return _context.Logs.Count();
        }

        // Returns last N logs ordered from newest.
        public List<Log> GetLatestLogs(int n)
        {
            // 1. load last n logs ordered by newest first
            return _context.Logs
                .OrderByDescending(x => x.Timestamp)
                .Take(n)
                .ToList();
        }

        // Adds one log row.
        public void AddLog(Log log)
        {
            // 1. add Log row
            _context.Logs.Add(log);
        }

        // Saves pending database changes.
        public int SaveChanges()
        {
            // 1. call SaveChanges
            return _context.SaveChanges();
        }
    }
}
