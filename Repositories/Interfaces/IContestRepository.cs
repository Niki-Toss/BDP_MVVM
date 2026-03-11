using System.Collections.Generic;
using System.Threading.Tasks;
using BDP_MVVM.Models;

namespace BDP_MVVM.Repositories.Interfaces
{
    // Репозиторий для работы с контестами (соревнованиями по программированию)
    public interface IContestRepository
    {
        // Базовые CRUD операции
        Task<List<Contest>> GetAllAsync();
        Task<Contest> GetByIdAsync(int contestId);
        Task<int> CreateAsync(Contest contest);
        Task<bool> UpdateAsync(Contest contest);
        Task<bool> DeleteAsync(int contestId);
        // Связи задачи с контестами (many-to-many через Task_Contest)
        Task<List<int>> GetContestIdsByTaskAsync(int taskId);
        Task<bool> SaveTaskContestsAsync(int taskId, IEnumerable<int> contestIds);
    }
}