using System.Windows;
using Microsoft.Win32;
using BDP_MVVM.Services.Interfaces;

namespace BDP_MVVM.Services
{
    // Сервис для показа MessageBox и диалогов выбора файлов
    public class DialogService : IDialogService
    {
        // Информационные сообщения (иконка i)
        public void ShowInfo(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        // Предупреждения (жёлтый треугольник)
        public void ShowWarning(string message, string title = "Внимание")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        // Ошибки (красный крестик)
        public void ShowError(string message, string title = "Ошибка")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        // Подтверждение действия (кнопки Да/Нет, возвращает true если Да)
        public bool ShowConfirmation(string message, string title = "Подтверждение")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
        // Диалог сохранения файла (возвращает выбранный путь или null при отмене)
        public string ShowSaveFileDialog(string filter = "All files (*.*)|*.*", string defaultFileName = "")
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                FileName = defaultFileName
            };
            if (dialog.ShowDialog() == true)
                return dialog.FileName;
            return null;
        }
    }
}