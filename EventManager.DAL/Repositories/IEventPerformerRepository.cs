using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public interface IEventPerformerRepository
    {
        List<EventPerformer> GetEventPerformersByEventId(int eventId);
        List<Performer> GetUnassignedPerformersForEvent(int eventId);
        bool EventPerformerRelationExists(int eventId, int performerId);
        EventPerformer? GetEventPerformerRelation(int eventId, int performerId);
        void AddEventPerformer(EventPerformer eventPerformer);
        void RemoveEventPerformer(EventPerformer existingRelation);
        int SaveChanges();
    }
}
