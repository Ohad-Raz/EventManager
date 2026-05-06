using EventManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.DAL.Repositories
{
    public class DbRegistrationRepository : IRegistrationRepository
    {
        private readonly EventManagerDbContext _context;

        public DbRegistrationRepository(EventManagerDbContext context)
        {
            _context = context;
        }

        // Returns one user by username.
        public User? GetUserByUsername(string username)
        {
            // 1. find logged-in user
            return _context.Users.FirstOrDefault(x => x.Username == username);
        }

        // Returns one event by id.
        public Event? GetEventById(int id)
        {
            // 1. find target event
            return _context.Events.FirstOrDefault(e => e.Id == id);
        }

        // Returns one registration for same user and event.
        public Registration? GetRegistrationByUserAndEvent(int userId, int eventId)
        {
            // 1. check duplicate registration for same user and event
            return _context.Registrations
                .FirstOrDefault(r => r.UserId == userId && r.EventId == eventId);
        }

        // Adds a new registration row.
        public void AddRegistration(Registration registration)
        {
            // 1. add Registration row
            _context.Registrations.Add(registration);
        }

        // Returns all registrations with user and event data.
        public List<Registration> GetAllRegistrationsWithDetails()
        {
            // 1. load registrations with related user and event
            return _context.Registrations
                .Include(x => x.User)
                .Include(x => x.Event)
                .ToList();
        }

        // Returns one registration by id with user and event data.
        public Registration? GetRegistrationByIdWithDetails(int id)
        {
            // 1. load registration with related user and event
            return _context.Registrations
                .Include(x => x.User)
                .Include(x => x.Event)
                .FirstOrDefault(x => x.Id == id);
        }

        // Returns registrations for one username with user and event data.
        public List<Registration> GetRegistrationsByUsernameWithDetails(string username)
        {
            // 1. load this user's registrations with related data
            return _context.Registrations
                .Include(x => x.User)
                .Include(x => x.Event)
                .Where(x => x.User.Username == username)
                .ToList();
        }

        // Saves pending database changes.
        public int SaveChanges()
        {
            // 1. call SaveChanges
            return _context.SaveChanges();
        }
    }
}
