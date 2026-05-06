using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public interface IEventRepository
    {
        Event? GetEventById(int id);
        List<Event> GetAllEvents();
        List<Event> SearchEvents(string? q, int? eventTypeId, int page, int count);

        User? GetUserByUsername(string username);
        EventType? GetEventTypeById(int id);
        Image? GetImageById(int id);

        void AddEvent(Event newEvent);
        void RemoveEvent(Event existingEvent);
        int SaveChanges();

        List<EventPerformer> GetEventPerformersByEventId(int eventId);
        Performer? GetPerformerById(int performerId);
        bool EventPerformerRelationExists(int eventId, int performerId);
        EventPerformer? GetEventPerformerRelation(int eventId, int performerId);
        void AddEventPerformer(EventPerformer eventPerformer);
        void RemoveEventPerformer(EventPerformer existingRelation);
    }
}
