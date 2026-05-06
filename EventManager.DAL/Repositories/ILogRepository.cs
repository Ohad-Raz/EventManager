using EventManager.DAL.Models;

namespace EventManager.DAL.Repositories
{
    public interface ILogRepository
    {
        int CountLogs();
        List<Log> GetLatestLogs(int n);
        void AddLog(Log log);
        int SaveChanges();
    }
}
