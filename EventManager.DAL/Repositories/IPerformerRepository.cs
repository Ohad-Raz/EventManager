using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public interface IPerformerRepository
    {
        List<Performer> GetAllPerformers();
        Performer? GetPerformerById(int id);
        void AddPerformer(Performer performer);
        void UpdatePerformer(Performer performer);
        void RemovePerformer(Performer performer);
        bool PerformerNameExists(string name, int? excludeId = null);
        bool PerformerHasEventAssignments(int performerId);
        int SaveChanges();
    }
}
