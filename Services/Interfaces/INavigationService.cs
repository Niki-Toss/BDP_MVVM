using BDP_MVVM.Models;

namespace BDP_MVVM.Services.Interfaces
{
    // Сервис для открытия модальных окон (редактирование задач и т.д.)
    public interface INavigationService
    {
        // Открывает окно редактирования задачи
        // Возвращает true если сохранили, false если отменили
        // Если task == null, создаёт новую задачу
        bool ShowTaskEditDialog(ProgrammingTask task = null);
    }
}