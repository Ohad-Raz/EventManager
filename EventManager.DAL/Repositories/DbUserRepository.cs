using EventManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.DAL.Repositories
{
    public class DbUserRepository : IUserRepository
    {
        private readonly EventManagerDbContext _context;

        public DbUserRepository(EventManagerDbContext context)
        {
            _context = context;
        }

        // Checks whether username already exists.
        public bool UsernameExists(string username)
        {
            // 1. check duplicate usernames
            return _context.Users.Any(x => x.Username == username);
        }

        // Checks whether email already exists, optionally excluding one user id.
        public bool EmailExists(string email, int? excludeUserId = null)
        {
            return _context.Users.Any(x =>
                x.Email == email &&
                (!excludeUserId.HasValue || x.Id != excludeUserId.Value));
        }

        // Adds a new user row.
        public void AddUser(User user)
        {
            // 1. add User row
            _context.Users.Add(user);
        }

        // Returns one user with Role by username.
        public User? GetUserWithRoleByUsername(string username)
        {
            // 1. find user and load Role for JWT
            return _context.Users
                .Include(x => x.Role)
                .FirstOrDefault(x => x.Username == username);
        }

        // Returns one user by username.
        public User? GetUserByUsername(string? username)
        {
            // 1. find user by username
            return _context.Users.FirstOrDefault(x => x.Username == username);
        }

        // Returns one user by id.
        public User? GetUserById(int id)
        {
            // 1. find target user by id
            return _context.Users.FirstOrDefault(x => x.Id == id);
        }

        // Returns all users for dropdowns.
        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        // Saves pending database changes.
        public int SaveChanges()
        {
            // 1. call SaveChanges
            return _context.SaveChanges();
        }
    }
}
