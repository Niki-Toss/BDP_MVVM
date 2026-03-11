using System.Windows;
using BDP_MVVM.ViewModels;

namespace BDP_MVVM.Views
{
    // Модальное окно редактирования задачи (создание новой или изменение существующей)
    // Открывается через NavigationService, возвращает DialogResult
    public partial class TaskEditWindow : Window
    {
        private TaskEditViewModel _viewModel;
        public TaskEditWindow(TaskEditViewModel viewModel)
        {
            InitializeComponent();
            // Устанавливаем ViewModel через конструктор (DI)
            _viewModel = viewModel;
            DataContext = _viewModel;
            // Подписываемся на событие закрытия окна из ViewModel
            // success = true если сохранили, false если отменили
            _viewModel.CloseRequested += OnCloseRequested;
        }
        // Обработчик события закрытия из ViewModel
        private void OnCloseRequested(bool success)
        {
            DialogResult = success;
        }
        // Отписываемся от события при закрытии (избегаем утечек памяти)
        protected override void OnClosed(System.EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.CloseRequested -= OnCloseRequested;
            }
            base.OnClosed(e);
        }
    }
}