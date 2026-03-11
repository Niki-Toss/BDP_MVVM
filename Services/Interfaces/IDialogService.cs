namespace BDP_MVVM.Services.Interfaces
{
    // Сервис для показа диалоговых окон (MessageBox и file dialogs)
    public interface IDialogService
    {
        // Информационные сообщения
        void ShowInfo(string message, string title = "Информация");
        void ShowWarning(string message, string title = "Внимание");
        void ShowError(string message, string title = "Ошибка");
        // Подтверждение действий (возвращает true если пользователь нажал "Да")
        bool ShowConfirmation(string message, string title = "Подтверждение");
        // Диалог сохранения файла (возвращает выбранный путь или null)
        string ShowSaveFileDialog(string filter = "All files (*.*)|*.*", string defaultFileName = "");
    }
}