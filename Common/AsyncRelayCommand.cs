using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BDP_MVVM.Common
{
    // Команда для асинхронных операций с защитой от двойного запуска.
    // Используется для загрузки данных, сохранения в БД и других async операций.
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Predicate<object> _canExecute;
        private bool _isExecuting;
        public AsyncRelayCommand(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        // WPF автоматически подписывается на это событие для обновления состояния кнопок
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            // Блокируем выполнение если команда уже запущена или условие не выполнено
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }
        public async void Execute(object parameter)
        {
            // Помечаем что команда выполняется (отключит кнопку)
            _isExecuting = true;
            RaiseCanExecuteChanged();
            try
            {
                await _execute(parameter);
            }
            finally
            {
                // Снимаем блокировку даже если было исключение
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}