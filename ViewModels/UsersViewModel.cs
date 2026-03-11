using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BDP_MVVM.Common;
using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;
using BDP_MVVM.Services.Interfaces;

namespace BDP_MVVM.ViewModels
{
    // ViewModel для управления пользователями системы
    // Доступен только администраторам и пользователям с правами управления
    // Поддерживает создание, редактирование и удаление пользователей
    public class UsersViewModel : ViewModelBase
    {
        #region Fields
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService _authService;
        private readonly IDialogService _dialogService;
        private User _selectedUser;
        private bool _isEditing;
        private bool _isCreating;
        private string _newЛогин;
        private string _newПароль;
        private string _newEmail;
        private string _newОписание;
        private Role _newРоль;
        private string _editЛогин;
        private string _editEmail;
        private string _editОписание;
        private Role _editРоль;
        private string _editПароль;
        #endregion
        #region Properties
        // Коллекция пользователей для отображения в списке
        public ObservableCollection<User> Users { get; }
        // Коллекция доступных ролей для выбора при создании/редактировании пользователя
        public ObservableCollection<Role> Roles { get; }
        // Выбранный пользователь в списке
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                if (value != null)
                {
                    // Заполняем поля редактирования данными выбранного пользователя
                    EditЛогин = value.Логин;
                    EditEmail = value.Email;
                    EditОписание = value.Описание;
                    EditРоль = Roles.FirstOrDefault(r => r.Role_ID == value.Role_ID);
                    EditПароль = string.Empty;
                }
                IsEditing = false;
                IsCreating = false;
            }
        }
        // Включен ли режим редактирования существующего пользователя
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                SetProperty(ref _isEditing, value);
                if (value) IsCreating = false;
            }
        }
        // Включен ли режим создания нового пользователя
        public bool IsCreating
        {
            get => _isCreating;
            set
            {
                SetProperty(ref _isCreating, value);
                if (value) IsEditing = false;
            }
        }
        // Есть ли пользователи в коллекции
        public bool HasUsers => Users.Count > 0;
        // Не активен ли режим редактирования или создания (для блокировки UI)
        public bool IsNotEditingOrCreating => !IsEditing && !IsCreating;
        // Логин для создания нового пользователя
        public string NewЛогин
        {
            get => _newЛогин;
            set => SetProperty(ref _newЛогин, value);
        }
        // Пароль для создания нового пользователя
        public string NewПароль
        {
            get => _newПароль;
            set => SetProperty(ref _newПароль, value);
        }
        // Email для создания нового пользователя
        public string NewEmail
        {
            get => _newEmail;
            set => SetProperty(ref _newEmail, value);
        }
        // Описание для создания нового пользователя
        public string NewОписание
        {
            get => _newОписание;
            set => SetProperty(ref _newОписание, value);
        }
        // Роль для создания нового пользователя
        public Role NewРоль
        {
            get => _newРоль;
            set => SetProperty(ref _newРоль, value);
        }
        // Логин для редактирования выбранного пользователя
        public string EditЛогин
        {
            get => _editЛогин;
            set => SetProperty(ref _editЛогин, value);
        }
        // Email для редактирования выбранного пользователя
        public string EditEmail
        {
            get => _editEmail;
            set => SetProperty(ref _editEmail, value);
        }
        // Описание для редактирования выбранного пользователя
        public string EditОписание
        {
            get => _editОписание;
            set => SetProperty(ref _editОписание, value);
        }
        // Роль для редактирования выбранного пользователя
        public Role EditРоль
        {
            get => _editРоль;
            set => SetProperty(ref _editРоль, value);
        }
        // Новый пароль для редактирования пользователя (обновляется только если не пустой)
        public string EditПароль
        {
            get => _editПароль;
            set => SetProperty(ref _editПароль, value);
        }
        #endregion
        #region Commands
        // Команда загрузки пользователей и ролей из базы данных
        public ICommand LoadCommand { get; private set; }
        // Команда отображения формы создания нового пользователя
        public ICommand ShowCreateCommand { get; private set; }
        // Команда создания нового пользователя
        public ICommand CreateCommand { get; private set; }
        // Команда перехода в режим редактирования выбранного пользователя
        public ICommand EditCommand { get; private set; }
        // Команда сохранения изменений пользователя
        public ICommand SaveEditCommand { get; private set; }
        // Команда отмены редактирования или создания
        public ICommand CancelCommand { get; private set; }
        // Команда удаления выбранного пользователя
        public ICommand DeleteCommand { get; private set; }
        #endregion
        #region Constructor
        // Инициализация ViewModel для управления пользователями
        public UsersViewModel(
            IUserRepository userRepository,
            IAuthenticationService authService,
            IDialogService dialogService)
        {
            _userRepository = userRepository;
            _authService = authService;
            _dialogService = dialogService;
            Users = new ObservableCollection<User>();
            Roles = new ObservableCollection<Role>();
            LoadCommand = new AsyncRelayCommand(async _ => await LoadAsync());
            // Показываем форму создания нового пользователя
            ShowCreateCommand = new RelayCommand(_ =>
            {
                SelectedUser = null;
                // Очищаем поля формы
                NewЛогин = string.Empty;
                NewПароль = string.Empty;
                NewEmail = string.Empty;
                NewОписание = string.Empty;
                NewРоль = Roles.FirstOrDefault();
                IsCreating = true;
                OnPropertyChanged(nameof(IsNotEditingOrCreating));
            });
            CreateCommand = new AsyncRelayCommand(
                async _ => await CreateAsync(),
                _ => !string.IsNullOrWhiteSpace(NewЛогин)
                     && !string.IsNullOrWhiteSpace(NewПароль)
                     && NewРоль != null);
            EditCommand = new RelayCommand(
                _ =>
                {
                    IsEditing = true;
                    OnPropertyChanged(nameof(IsNotEditingOrCreating));
                },
                _ => SelectedUser != null);
            SaveEditCommand = new AsyncRelayCommand(
                async _ => await SaveEditAsync(),
                _ => !string.IsNullOrWhiteSpace(EditЛогин) && EditРоль != null);
            CancelCommand = new RelayCommand(_ =>
            {
                IsEditing = false;
                IsCreating = false;
                OnPropertyChanged(nameof(IsNotEditingOrCreating));
            });
            DeleteCommand = new AsyncRelayCommand(
                async _ => await DeleteAsync(),
                _ => SelectedUser != null);
            // Загружаем данные при создании ViewModel
            _ = LoadAsync();
        }
        #endregion
        #region Private Methods
        // Загрузить все роли и пользователей из базы данных
        private async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                // Сначала загружаем роли (нужны для ComboBox)
                var roles = await _userRepository.GetAllRolesAsync();
                Roles.Clear();
                foreach (var r in roles)
                    Roles.Add(r);
                // Затем загружаем пользователей
                var users = await _userRepository.GetAllAsync();
                Users.Clear();
                foreach (var u in users)
                    Users.Add(u);
                OnPropertyChanged(nameof(HasUsers));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
        // Создать нового пользователя с проверкой уникальности логина
        // Пароль автоматически хешируется в Repository
        private async Task CreateAsync()
        {
            // Проверяем уникальность логина
            if (await _userRepository.LoginExistsAsync(NewЛогин))
            {
                _dialogService.ShowError($"Логин \"{NewЛогин}\" уже занят.");
                return;
            }
            IsLoading = true;
            try
            {
                var user = new User
                {
                    Логин = NewЛогин.Trim(),
                    Email = NewEmail?.Trim(),
                    Описание = NewОписание?.Trim(),
                    Role_ID = NewРоль.Role_ID
                };
                // Пароль хешируется автоматически в Repository
                int newId = await _userRepository.CreateAsync(user, NewПароль);
                if (newId > 0)
                {
                    user.User_ID = newId;
                    user.Роль = NewРоль;
                    user.Дата_создания = DateTime.Now;
                    Users.Add(user);
                    IsCreating = false;
                    OnPropertyChanged(nameof(HasUsers));
                    OnPropertyChanged(nameof(IsNotEditingOrCreating));
                    _dialogService.ShowInfo($"Пользователь \"{user.Логин}\" создан!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось создать пользователя.");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        // Сохранить изменения выбранного пользователя
        // Пароль обновляется только если введён новый
        private async Task SaveEditAsync()
        {
            if (SelectedUser == null) return;
            // Проверяем уникальность логина (исключая текущего пользователя)
            if (await _userRepository.LoginExistsAsync(EditЛогин, SelectedUser.User_ID))
            {
                _dialogService.ShowError($"Логин \"{EditЛогин}\" уже занят.");
                return;
            }
            IsLoading = true;
            try
            {
                SelectedUser.Логин = EditЛогин.Trim();
                SelectedUser.Email = EditEmail?.Trim();
                SelectedUser.Описание = EditОписание?.Trim();
                SelectedUser.Role_ID = EditРоль.Role_ID;
                SelectedUser.Роль = EditРоль;
                bool success = await _userRepository.UpdateAsync(SelectedUser);
                if (success)
                {
                    // Обновляем пароль только если введён новый
                    if (!string.IsNullOrWhiteSpace(EditПароль))
                        await _userRepository.UpdatePasswordAsync(SelectedUser.User_ID, EditПароль);
                    // Обновляем элемент в коллекции для триггера UI
                    int index = Users.IndexOf(SelectedUser);
                    if (index >= 0)
                    {
                        Users[index] = SelectedUser;
                        SelectedUser = Users[index];
                    }
                    IsEditing = false;
                    OnPropertyChanged(nameof(IsNotEditingOrCreating));
                    _dialogService.ShowInfo("Пользователь обновлён!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось обновить пользователя.");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        // Удалить выбранного пользователя с подтверждением
        private async Task DeleteAsync()
        {
            if (SelectedUser == null) return;
            if (!_dialogService.ShowConfirmation(
                $"Удалить пользователя \"{SelectedUser.Логин}\"?", "Подтверждение"))
                return;
            IsLoading = true;
            try
            {
                bool success = await _userRepository.DeleteAsync(SelectedUser.User_ID);
                if (success)
                {
                    Users.Remove(SelectedUser);
                    SelectedUser = null;
                    IsEditing = false;
                    IsCreating = false;
                    OnPropertyChanged(nameof(HasUsers));
                    OnPropertyChanged(nameof(IsNotEditingOrCreating));
                    _dialogService.ShowInfo("Пользователь удалён!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось удалить пользователя.");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion
    }
}