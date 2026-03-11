using System.Windows;
using BDP_MVVM.ViewModels;

namespace BDP_MVVM.Views
{
    // Главное окно приложения с боковым меню навигации
    // Управляет переключением между разделами (задачи, справочники, отчёты)
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            // Получаем ViewModel из DI
            _viewModel = App.GetService<MainViewModel>();
            DataContext = _viewModel;
            // Подписываемся на события
            _viewModel.LogoutRequested += OnLogoutRequested;
            _viewModel.NavigationRequested += OnNavigationRequested;
            // Открываем стартовую страницу
            OnNavigationRequested("Tasks");
        }
        // Переключение между страницами - создаём View и ViewModel для каждой
        private void OnNavigationRequested(string page)
        {
            switch (page)
            {
                case "Tasks":
                    var tasksViewModel = App.GetService<TasksViewModel>();
                    MainContent.Content = new TasksView { DataContext = tasksViewModel };
                    break;
                case "Tags":
                    var tagsViewModel = App.GetService<TagsViewModel>();
                    MainContent.Content = new TagsView { DataContext = tagsViewModel };
                    break;
                case "Contests":
                    var contestsViewModel = App.GetService<ContestsViewModel>();
                    MainContent.Content = new ContestsView { DataContext = contestsViewModel };
                    break;
                case "Platforms":
                    var platformsViewModel = App.GetService<PlatformsViewModel>();
                    MainContent.Content = new PlatformsView { DataContext = platformsViewModel };
                    break;
                case "Users":
                    var usersViewModel = App.GetService<UsersViewModel>();
                    MainContent.Content = new UsersView { DataContext = usersViewModel };
                    break;
                case "Reports":
                    var reportsViewModel = App.GetService<ReportsViewModel>();
                    MainContent.Content = new ReportsView { DataContext = reportsViewModel };
                    break;
            }
        }
        // Выход - открываем окно входа и закрываем главное окно
        private void OnLogoutRequested()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
        // Отписываемся от событий при закрытии (избегаем утечек памяти)
        protected override void OnClosed(System.EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.LogoutRequested -= OnLogoutRequested;
                _viewModel.NavigationRequested -= OnNavigationRequested;
            }
            base.OnClosed(e);
        }
    }
}