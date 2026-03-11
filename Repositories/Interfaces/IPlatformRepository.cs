using System.Collections.Generic;
using System.Threading.Tasks;
using BDP_MVVM.Models;

namespace BDP_MVVM.Repositories.Interfaces
{
    // Вспомогательный класс для хранения связи задачи с платформой и статуса готовности
    public class TaskPlatformItem
    {
        public int PlatformId { get; set; }
        public bool Готовность { get; set; }
    }
    // Репозиторий для работы с платформами автопроверки (Codeforces, Yandex Contest и т.д.)
    public interface IPlatformRepository
    {
        // Базовые CRUD операции для справочника платформ
        Task<List<Platform>> GetAllAsync();
        Task<Platform> GetByIdAsync(int platformId);
        Task<int> CreateAsync(Platform platform);
        Task<bool> UpdateAsync(Platform platform);
        Task<bool> DeleteAsync(int platformId);
        // Проверка использования платформы перед удалением
        Task<bool> IsUsedInTasksAsync(int platformId);
        // Связи задачи с платформами (many-to-many через Task_Platform)
        Task<List<TaskPlatformItem>> GetPlatformsByTaskAsync(int taskId);
        Task<bool> SaveTaskPlatformsAsync(int taskId, IEnumerable<TaskPlatformItem> items);
    }
}