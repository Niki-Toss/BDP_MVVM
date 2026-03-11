using System.Windows.Controls;

namespace BDP_MVVM.Views
{
    // Главная страница приложения со списком задач
    // Включает систему фильтрации по тегам, платформам, контестам и сложности
    // Code-behind пустой - вся логика в TasksViewModel
    public partial class TasksView : UserControl
    {
        public TasksView()
        {
            InitializeComponent();
        }
    }
}