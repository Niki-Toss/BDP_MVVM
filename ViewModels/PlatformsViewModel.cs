using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BDP_MVVM.Common;
using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;
using BDP_MVVM.Services.Interfaces;

namespace BDP_MVVM.ViewModels
{
    // ViewModel для управления справочником платформ автопроверки
    // Поддерживает CRUD операции и проверку использования платформ перед удалением
    public class PlatformsViewModel : ViewModelBase
    {
        #region Fields
        private readonly IPlatformRepository _platformRepository;
        private readonly IAuthenticationService _authService;
        private readonly IDialogService _dialogService;
        private Platform _selectedPlatform;
        private bool _isEditing;
        private string _newНазвание;
        private bool _newАвто;
        private string _editНазвание;
        private bool _editАвто;
        #endregion
        #region Properties
        // Коллекция платформ для отображения в UI
        public ObservableCollection<Platform> Platforms { get; }
        // Выбранная платформа в списке
        public Platform SelectedPlatform
        {
            get => _selectedPlatform;
            set
            {
                SetProperty(ref _selectedPlatform, value);
                if (value != null)
                {
                    EditНазвание = value.Название;
                    EditАвто = value.Автопроверка_готовности;
                }
                IsEditing = false;
            }
        }
        // Включен ли режим редактирования
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }
        // Есть ли платформы в коллекции
        public bool HasPlatforms => Platforms.Count > 0;
        // Может ли пользователь модифицировать платформы (проверка прав)
        public bool CanModify => _authService?.CanEditTasks() ?? false;
        // Название для создания новой платформы
        public string NewНазвание
        {
            get => _newНазвание;
            set => SetProperty(ref _newНазвание, value);
        }
        // Флаг автопроверки готовности для новой платформы
        public bool NewАвто
        {
            get => _newАвто;
            set => SetProperty(ref _newАвто, value);
        }
        // Название для редактирования выбранной платформы
        public string EditНазвание
        {
            get => _editНазвание;
            set => SetProperty(ref _editНазвание, value);
        }
        // Флаг автопроверки готовности для редактирования платформы
        public bool EditАвто
        {
            get => _editАвто;
            set => SetProperty(ref _editАвто, value);
        }
        #endregion
        #region Commands
        // Команда загрузки платформ из базы данных
        public ICommand LoadCommand { get; private set; }
        // Команда создания новой платформы
        public ICommand AddCommand { get; private set; }
        // Команда перехода в режим редактирования
        public ICommand EditCommand { get; private set; }
        // Команда сохранения изменений платформы
        public ICommand SaveEditCommand { get; private set; }
        // Команда отмены редактирования
        public ICommand CancelEditCommand { get; private set; }
        // Команда удаления платформы
        public ICommand DeleteCommand { get; private set; }
        #endregion
        #region Constructor
        // Инициализация ViewModel для управления платформами
        public PlatformsViewModel(
            IPlatformRepository platformRepository,
            IAuthenticationService authService,
            IDialogService dialogService)
        {
            _platformRepository = platformRepository;
            _authService = authService;
            _dialogService = dialogService;
            Platforms = new ObservableCollection<Platform>();
            // Инициализируем команды с проверкой прав доступа
            LoadCommand = new AsyncRelayCommand(async _ => await LoadAsync());
            AddCommand = new AsyncRelayCommand(
                async _ => await AddAsync(),
                _ => !string.IsNullOrWhiteSpace(NewНазвание) && CanModify);
            EditCommand = new RelayCommand(
                _ => StartEdit(),
                _ => SelectedPlatform != null && CanModify);
            SaveEditCommand = new AsyncRelayCommand(
                async _ => await SaveEditAsync(),
                _ => !string.IsNullOrWhiteSpace(EditНазвание) && IsEditing);
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            DeleteCommand = new AsyncRelayCommand(
                async _ => await DeleteAsync(),
                _ => SelectedPlatform != null && CanModify);
            // Загружаем данные при создании ViewModel
            _ = LoadAsync();
        }
        #endregion
        #region Private Methods
        // Загрузить все платформы из базы данных
        private async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var platforms = await _platformRepository.GetAllAsync();
                Platforms.Clear();
                foreach (var p in platforms)
                    Platforms.Add(p);
                OnPropertyChanged(nameof(HasPlatforms));
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
        // Создать новую платформу
        private async Task AddAsync()
        {
            IsLoading = true;
            try
            {
                var platform = new Platform
                {
                    Название = NewНазвание.Trim(),
                    Автопроверка_готовности = NewАвто
                };
                int newId = await _platformRepository.CreateAsync(platform);
                if (newId > 0)
                {
                    platform.Platform_ID = newId;
                    Platforms.Add(platform);
                    // Очищаем поля формы
                    NewНазвание = string.Empty;
                    NewАвто = false;
                    OnPropertyChanged(nameof(HasPlatforms));
                    _dialogService.ShowInfo($"Платформа \"{platform.Название}\" создана!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось создать платформу.");
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
        // Перейти в режим редактирования выбранной платформы
        private void StartEdit()
        {
            if (SelectedPlatform == null) return;
            EditНазвание = SelectedPlatform.Название;
            EditАвто = SelectedPlatform.Автопроверка_готовности;
            IsEditing = true;
        }
        // Сохранить изменения платформы в базу данных
        private async Task SaveEditAsync()
        {
            if (SelectedPlatform == null) return;
            IsLoading = true;
            try
            {
                SelectedPlatform.Название = EditНазвание.Trim();
                SelectedPlatform.Автопроверка_готовности = EditАвто;
                bool success = await _platformRepository.UpdateAsync(SelectedPlatform);
                if (success)
                {
                    // Обновляем элемент в коллекции для триггера UI
                    int index = Platforms.IndexOf(SelectedPlatform);
                    if (index >= 0)
                    {
                        Platforms[index] = SelectedPlatform;
                        SelectedPlatform = Platforms[index];
                    }
                    IsEditing = false;
                    _dialogService.ShowInfo("Платформа обновлена!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось обновить платформу.");
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
        // Отменить редактирование платформы
        private void CancelEdit()
        {
            IsEditing = false;
        }
        // Удалить выбранную платформу с предупреждением о связанных задачах
        private async Task DeleteAsync()
        {
            if (SelectedPlatform == null) return;
            // Проверяем используется ли платформа
            bool isUsed = await _platformRepository.IsUsedInTasksAsync(SelectedPlatform.Platform_ID);
            string message = isUsed
                ? $"Платформа \"{SelectedPlatform.Название}\" используется в задачах.\n\nВсё равно удалить?"
                : $"Удалить платформу \"{SelectedPlatform.Название}\"?";
            if (!_dialogService.ShowConfirmation(message, "Подтверждение"))
                return;
            IsLoading = true;
            try
            {
                bool success = await _platformRepository.DeleteAsync(SelectedPlatform.Platform_ID);
                if (success)
                {
                    Platforms.Remove(SelectedPlatform);
                    SelectedPlatform = null;
                    IsEditing = false;
                    OnPropertyChanged(nameof(HasPlatforms));
                    _dialogService.ShowInfo("Платформа удалена!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось удалить платформу.");
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