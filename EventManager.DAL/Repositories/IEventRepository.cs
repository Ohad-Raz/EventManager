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

        User? GetUserByUsername(string username);
        EventType? GetEventTypeById(int id);
        Image? GetImageById(int id);
        List<User> GetAllUsers();
        List<EventType> GetAllEventTypes();
        List<Image> GetAllImages();
        void AddEventType(EventType eventType);
        void UpdateEventType(EventType eventType);
        void RemoveEventType(EventType eventType);
        bool EventTypeNameExists(string name, int? excludeId = null);
        bool EventTypeHasEvents(int eventTypeId);

        void AddEvent(Event newEvent);
        void UpdateEvent(Event existingEvent);
        void RemoveEvent(Event existingEvent);
        int SaveChanges();

        List<EventPerformer> GetEventPerformersByEventId(int eventId);
        List<Performer> GetUnassignedPerformersForEvent(int eventId);
        Performer? GetPerformerById(int performerId);
        bool EventPerformerRelationExists(int eventId, int performerId);
        EventPerformer? GetEventPerformerRelation(int eventId, int performerId);
        void AddEventPerformer(EventPerformer eventPerformer);
        void RemoveEventPerformer(EventPerformer existingRelation);
    }
}
