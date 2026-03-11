using System.Collections.Generic;
using System.Threading.Tasks;
using BDP_MVVM.Models;

namespace BDP_MVVM.Repositories.Interfaces
{
    // Репозиторий для работы с задачами по программированию - основная сущность приложения
    public interface ITaskRepository
    {
        // Получение задач (с фильтрацией или без)
        Task<List<ProgrammingTask>> GetAllAsync();
        Task<List<ProgrammingTask>> FilterAsync(int? minDifficulty, int? maxDifficulty, List<int> tagIds = null);
        Task<ProgrammingTask> GetByIdAsync(int taskId);
        // CRUD операции (создание/обновление автоматически сохраняют связи с тегами и контестами)
        Task<int> CreateAsync(ProgrammingTask task, List<int> tagIds = null, List<int> contestIds = null);
        Task<bool> UpdateAsync(ProgrammingTask task, List<int> tagIds = null, List<int> contestIds = null);
        Task<bool> DeleteAsync(int taskId);
        // Получение связанных данных для отображения в карточке задачи
        Task<List<Tag>> GetTagsForTaskAsync(int taskId);
        Task<List<Contest>> GetContestsForTaskAsync(int taskId);
    }
}