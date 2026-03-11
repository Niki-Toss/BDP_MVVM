using System.Collections.Generic;
using System.Threading.Tasks;
using BDP_MVVM.Models;

namespace BDP_MVVM.Repositories.Interfaces
{
    // Репозиторий для работы с тегами задач (Графы, ДП, Жадные алгоритмы и т.д.)
    public interface ITagRepository
    {
        // Базовые CRUD операции для справочника тегов
        Task<List<Tag>> GetAllAsync();
        Task<Tag> GetByIdAsync(int tagId);
        Task<int> CreateAsync(Tag tag);
        Task<bool> UpdateAsync(Tag tag);
        Task<bool> DeleteAsync(int tagId);
        // Проверка использования тега перед удалением
        Task<bool> IsUsedInTasksAsync(int tagId);
        // Связи задачи с тегами (many-to-many через Task_Tag)
        Task<List<int>> GetTagIdsByTaskAsync(int taskId);
        Task<bool> SaveTaskTagsAsync(int taskId, IEnumerable<int> tagIds);
    }
}