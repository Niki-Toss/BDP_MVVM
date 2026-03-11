using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;
using BDP_MVVM.Services.Interfaces;
using BDP_MVVM.ViewModels;
using BDP_MVVM.Views;
using System.Windows;

namespace BDP_MVVM.Services
{
    // Сервис для открытия модальных окон приложения
    public class NavigationService : INavigationService
    {
        // Открываем окно редактирования задачи (создание или изменение)
        // Возвращает true если пользователь сохранил, false если отменил
        public bool ShowTaskEditDialog(ProgrammingTask task = null)
        {
            // Получаем все необходимые зависимости из DI контейнера
            var tagRepo = App.GetService<ITagRepository>();
            var platformRepo = App.GetService<IPlatformRepository>();
            var contestRepo = App.GetService<IContestRepository>();
            var taskRepo = App.GetService<ITaskRepository>();
            var dialogService = App.GetService<IDialogService>();
            // Создаём ViewModel в зависимости от режима (создание/редактирование)
            TaskEditViewModel viewModel;
            if (task == null)
            {
                // Режим создания новой задачи
                viewModel = new TaskEditViewModel(
                    taskRepo, tagRepo, platformRepo, contestRepo, dialogService);
            }
            else
            {
                // Режим редактирования существующей задачи
                viewModel = new TaskEditViewModel(
                    task, taskRepo, tagRepo, platformRepo, contestRepo, dialogService);
            }
            var window = new TaskEditWindow(viewModel);
            // Устанавливаем Owner чтобы окно было модальным и центрировалось
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null && mainWindow != window)
            {
                window.Owner = mainWindow;
            }
            // ShowDialog() блокирует UI до закрытия окна
            return window.ShowDialog() == true;
        }
    }
}