using System.Windows;
using System.Windows.Controls;
using BDP_MVVM.ViewModels;

namespace BDP_MVVM.Views
{
    // Страница управления пользователями (только для администраторов)
    // Code-behind содержит минимальную логику для работы с PasswordBox
    // (PasswordBox не поддерживает биндинг Password по соображениям безопасности)
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
            // Подписываемся на PasswordBox после загрузки контрола
            Loaded += UsersView_Loaded;
        }
        // Подписываемся на события PasswordBox (они доступны только после InitializeComponent)
        private void UsersView_Loaded(object sender, RoutedEventArgs e)
        {
            if (NewPasswordBox != null)
            {
                NewPasswordBox.PasswordChanged += NewPasswordBox_PasswordChanged;
            }
            if (EditPasswordBox != null)
            {
                EditPasswordBox.PasswordChanged += EditPasswordBox_PasswordChanged;
            }
        }
        // Передаём пароль из формы создания в ViewModel
        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel vm && sender is PasswordBox pb)
            {
                vm.NewПароль = pb.Password;
            }
        }
        // Передаём пароль из формы редактирования в ViewModel
        private void EditPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel vm && sender is PasswordBox pb)
            {
                vm.EditПароль = pb.Password;
            }
        }
    }
}