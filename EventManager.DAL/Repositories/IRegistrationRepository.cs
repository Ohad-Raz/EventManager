using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public interface IRegistrationRepository
    {
        User? GetUserByUsername(string username);
        Event? GetEventById(int id);
        Registration? GetRegistrationByUserAndEvent(int userId, int eventId);
        void AddRegistration(Registration registration);
        List<Registration> GetAllRegistrationsWithDetails();
        Registration? GetRegistrationByIdWithDetails(int id);
        List<Registration> GetRegistrationsByUsernameWithDetails(string username);
        int SaveChanges();
    }
}
