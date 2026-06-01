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
            // 1. find target event only if not soft-deleted
            return _context.Events.FirstOrDefault(e => e.Id == id && e.DeletedAt == null);
        }

        // Returns existing registration for the same user and event, active or inactive.
        public Registration? GetRegistrationByUserAndEvent(int userId, int eventId)
        {
            // 1. find existing registration relation for same user and event
            return _context.Registrations
                .FirstOrDefault(r => r.UserId == userId && r.EventId == eventId);
        }

        // Returns the active registration for this user and event, or null.
        public Registration? GetActiveRegistrationByUserAndEvent(int userId, int eventId)
        {
            return _context.Registrations.FirstOrDefault(r =>
                r.UserId == userId &&
                r.EventId == eventId &&
                r.IsActive);
        }

        // Returns true when the user already has an active registration for this event.
        public bool UserIsActivelyRegistered(int userId, int eventId)
        {
            return GetActiveRegistrationByUserAndEvent(userId, eventId) != null;
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

        // Returns active registrations for one username with user and event data.
        public List<Registration> GetRegistrationsByUsernameWithDetails(string username)
        {
            // 1. load this user's active registrations with related data
            return _context.Registrations
                .Include(x => x.User)
                .Include(x => x.Event)
                .Where(x =>
                    x.User.Username == username &&
                    x.IsActive &&
                    x.Event.DeletedAt == null)
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
