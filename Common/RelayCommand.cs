using System;
using System.Windows.Input;

namespace BDP_MVVM.Common
{
    // Команда для обычных синхронных операций (клики по кнопкам, навигация и т.д.).
    // Для async операций AsyncRelayCommand.
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        // WPF подписывается на это событие чтобы знать когда перепроверить CanExecute.
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        // Определяет доступна ли кнопка (enabled/disabled).
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        // Выполняется при клике на кнопку.
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        // Принудительно обновляет состояние всех кнопок с командами.
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}