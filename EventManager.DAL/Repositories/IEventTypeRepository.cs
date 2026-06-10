using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public interface IEventTypeRepository
    {
        EventType? GetEventTypeById(int id);
        List<EventType> GetAllEventTypes();
        void AddEventType(EventType eventType);
        void UpdateEventType(EventType eventType);
        void RemoveEventType(EventType eventType);
        bool EventTypeNameExists(string name, int? excludeId = null);
        bool EventTypeHasEvents(int eventTypeId);
        int SaveChanges();
    }
}
