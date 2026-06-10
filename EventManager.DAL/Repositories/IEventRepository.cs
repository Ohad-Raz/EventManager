using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public interface IEventRepository
    {
        Event? GetEventById(int id);
        Event? GetEventByIdWithDetails(int id);
        List<Event> GetAllEvents();
        List<Event> GetAllEventsWithDetails();
        List<Event> SearchEvents(string? q, int? eventTypeId, int page, int count);
        bool EventExists(int id);

        Image? GetImageById(int id);
        List<Image> GetAllImages();

        void AddEvent(Event newEvent);
        void UpdateEvent(Event existingEvent);
        void RemoveEvent(Event existingEvent);
        int SaveChanges();
    }
}
