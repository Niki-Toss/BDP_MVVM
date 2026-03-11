using System;
using System.Threading.Tasks;

namespace BDP_MVVM.Common
{
    // Базовый класс для всех ViewModel. Содержит общую логику загрузки и обработки ошибок.
    public class ViewModelBase : ObservableObject
    {
        private bool _isLoading;
        private string _errorMessage;
        private bool _hasError;
        // Показывает индикатор загрузки в UI (спиннер/прогресс-бар).
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        // Текст ошибки для показа пользователю.
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    // Автоматически обновляем флаг HasError.
                    HasError = !string.IsNullOrEmpty(value);
                }
            }
        }
        // Флаг наличия ошибки (для показа/скрытия блока ошибки в UI).
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }
        // Выполняет async операцию с автоматической обработкой загрузки и ошибок.
        // Включает IsLoading, выполняет действие, перехватывает исключения.
        protected async Task ExecuteAsync(Func<Task> action, string errorMessage = "Произошла ошибка")
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"{errorMessage}: {ex.Message}";
            }
            finally
            {
                // Всегда выключаем индикатор загрузки, даже если была ошибка.
                IsLoading = false;
            }
        }
    }
}