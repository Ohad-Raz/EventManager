using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public interface IUserRepository
    {
        bool UsernameExists(string username);
        bool EmailExists(string email);
        void AddUser(User user);
        User? GetUserWithRoleByUsername(string username);
        User? GetUserByUsername(string? username);
        User? GetUserById(int id);
        int SaveChanges();
    }
}
