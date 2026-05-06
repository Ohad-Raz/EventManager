using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public interface IPerformerRepository
    {
        List<Performer> GetAllPerformers();
        Performer? GetPerformerById(int id);
        void AddPerformer(Performer performer);
        void RemovePerformer(Performer performer);
        int SaveChanges();
    }
}
