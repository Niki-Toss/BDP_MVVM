using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using BDP_MVVM.Models;

namespace BDP_MVVM.Views
{
    // Окно просмотра задачи в режиме "только чтение"
    // DataContext устанавливается напрямую на модель ProgrammingTask
    public partial class TaskViewWindow : Window
    {
        public TaskViewWindow(ProgrammingTask task)
        {
            // Проверяем что задача передана
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task), "Задача не может быть null");
            }
            InitializeComponent();
            // Устанавливаем задачу как DataContext (биндинги в XAML работают напрямую с моделью)
            DataContext = task;
        }
        // Закрытие окна по кнопке
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        // Открытие ссылки Polygon в браузере
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                if (e.Uri != null && !string.IsNullOrEmpty(e.Uri.AbsoluteUri))
                {
                    // Открываем ссылку в браузере по умолчанию
                    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось открыть ссылку:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                e.Handled = true;
            }
        }
    }
}