using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using BDP_MVVM.Common;
using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;
using BDP_MVVM.Services;
using BDP_MVVM.Services.Interfaces;

namespace BDP_MVVM.ViewModels
{
    // ViewModel страницы задач - основная страница приложения
    // Управляет списком задач с системой многокритериальной фильтрации
    public class TasksViewModel : ViewModelBase
    {
        #region Nested Classes
        // Элемент фильтра с чекбоксом (для тегов, платформ, контестов)
        public class FilterItem : ViewModelBase
        {
            private bool _isSelected;
            // ID сущности (Tag_ID, Platform_ID или Contest_ID)
            public int Id { get; set; }
            // Название для отображения в UI
            public string Name { get; set; }
            // Выбран ли элемент фильтра
            public bool IsSelected
            {
                get => _isSelected;
                set => SetProperty(ref _isSelected, value);
            }
        }
        #endregion
        #region Fields
        private readonly ITaskRepository _taskRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IPlatformRepository _platformRepository;
        private readonly IContestRepository _contestRepository;
        private readonly NavigationService _navigationService;
        private readonly IAuthenticationService _authService;
        private readonly IDialogService _dialogService;
        private ProgrammingTask _selectedTask;
        private bool _isLoadingData;
        private int _minDifficulty = 1;
        private int _maxDifficulty = 10;
        private bool _showWithoutTag;
        private bool _showWithoutPlatform;
        private bool _showWithoutContest;
        #endregion
        #region Properties
        // Коллекция задач для отображения
        public ObservableCollection<ProgrammingTask> Tasks { get; }
        // Элементы фильтра по тегам
        public ObservableCollection<FilterItem> FilterTagItems { get; }
        // Элементы фильтра по платформам
        public ObservableCollection<FilterItem> FilterPlatformItems { get; }
        // Элементы фильтра по контестам
        public ObservableCollection<FilterItem> FilterContestItems { get; }
        // View с применением фильтрации
        public CollectionViewSource TasksView { get; }
        // Выбранная задача в списке
        public ProgrammingTask SelectedTask
        {
            get => _selectedTask;
            set => SetProperty(ref _selectedTask, value);
        }
        // Минимальный уровень сложности для фильтра (1-10)
        public int MinDifficulty
        {
            get => _minDifficulty;
            set
            {
                if (value > MaxDifficulty)
                    value = MaxDifficulty;
                SetProperty(ref _minDifficulty, value);
                ApplyFilters();
            }
        }
        // Максимальный уровень сложности для фильтра (1-10)
        public int MaxDifficulty
        {
            get => _maxDifficulty;
            set
            {
                if (value < MinDifficulty)
                    value = MinDifficulty;
                SetProperty(ref _maxDifficulty, value);
                ApplyFilters();
            }
        }
        // Показывать задачи без тегов
        public bool ShowWithoutTag
        {
            get => _showWithoutTag;
            set
            {
                SetProperty(ref _showWithoutTag, value);
                ApplyFilters();
                OnPropertyChanged(nameof(SelectedTagsText));
            }
        }
        // Показывать задачи без платформ
        public bool ShowWithoutPlatform
        {
            get => _showWithoutPlatform;
            set
            {
                SetProperty(ref _showWithoutPlatform, value);
                ApplyFilters();
                OnPropertyChanged(nameof(SelectedPlatformsText));
            }
        }
        // Показывать задачи без контестов
        public bool ShowWithoutContest
        {
            get => _showWithoutContest;
            set
            {
                SetProperty(ref _showWithoutContest, value);
                ApplyFilters();
                OnPropertyChanged(nameof(SelectedContestsText));
            }
        }
        // Текстовое описание выбранных тегов для UI
        public string SelectedTagsText
        {
            get
            {
                var count = FilterTagItems.Count(x => x.IsSelected);
                if (count == 0 && !ShowWithoutTag)
                    return "Все теги";
                var parts = new List<string>();
                if (count > 0)
                    parts.Add($"{count} тег(ов)");
                if (ShowWithoutTag)
                    parts.Add("Без тега");
                return string.Join(", ", parts);
            }
        }
        // Текстовое описание выбранных платформ для UI
        public string SelectedPlatformsText
        {
            get
            {
                var count = FilterPlatformItems.Count(x => x.IsSelected);
                if (count == 0 && !ShowWithoutPlatform)
                    return "Все платформы";
                var parts = new List<string>();
                if (count > 0)
                    parts.Add($"{count} платформ(ы)");
                if (ShowWithoutPlatform)
                    parts.Add("Без платформы");
                return string.Join(", ", parts);
            }
        }
        // Текстовое описание выбранных контестов для UI
        public string SelectedContestsText
        {
            get
            {
                var count = FilterContestItems.Count(x => x.IsSelected);
                if (count == 0 && !ShowWithoutContest)
                    return "Все контесты";
                var parts = new List<string>();
                if (count > 0)
                    parts.Add($"{count} контест(ов)");
                if (ShowWithoutContest)
                    parts.Add("Без контеста");
                return string.Join(", ", parts);
            }
        }
        // Может ли текущий пользователь редактировать задачи
        public bool CanEdit => _authService?.CanEditTasks() ?? false;
        // Может ли текущий пользователь удалять задачи
        public bool CanDelete => _authService?.CanDeleteTasks() ?? false;
        #endregion
        #region Commands
        // Команда загрузки задач из базы данных
        public ICommand LoadTasksCommand { get; private set; }
        // Команда создания новой задачи
        public ICommand AddTaskCommand { get; private set; }
        // Команда редактирования выбранной задачи
        public ICommand EditTaskCommand { get; private set; }
        // Команда удаления выбранной задачи
        public ICommand DeleteTaskCommand { get; private set; }
        // Команда просмотра задачи в отдельном окне
        public ICommand ViewTaskCommand { get; private set; }
        // Команда сброса всех фильтров
        public ICommand ClearFiltersCommand { get; private set; }
        #endregion
        #region Constructor
        // Инициализация ViewModel для страницы задач
        public TasksViewModel(
            ITaskRepository taskRepository,
            ITagRepository tagRepository,
            IPlatformRepository platformRepository,
            IContestRepository contestRepository,
            NavigationService navigationService,
            IAuthenticationService authService,
            IDialogService dialogService)
        {
            _taskRepository = taskRepository;
            _tagRepository = tagRepository;
            _platformRepository = platformRepository;
            _contestRepository = contestRepository;
            _navigationService = navigationService;
            _authService = authService;
            _dialogService = dialogService;
            Tasks = new ObservableCollection<ProgrammingTask>();
            FilterTagItems = new ObservableCollection<FilterItem>();
            FilterPlatformItems = new ObservableCollection<FilterItem>();
            FilterContestItems = new ObservableCollection<FilterItem>();
            // Настраиваем View с фильтрацией
            TasksView = new CollectionViewSource { Source = Tasks };
            TasksView.Filter += TasksView_Filter;
            // Инициализируем команды с проверкой прав доступа
            LoadTasksCommand = new AsyncRelayCommand(async _ => await LoadTasksAsync());
            AddTaskCommand = new RelayCommand(_ => AddTask(), _ => CanEdit);
            ViewTaskCommand = new RelayCommand(param => ViewTask(param as ProgrammingTask));
            EditTaskCommand = new RelayCommand(param => EditTask(param as ProgrammingTask), _ => CanEdit);
            DeleteTaskCommand = new AsyncRelayCommand(async param => await DeleteTaskAsync(param as ProgrammingTask), _ => CanDelete);
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            // Загружаем задачи и фильтры при создании ViewModel
            _ = SafeInitializeAsync();
        }
        #endregion
        #region Private Methods - Initialization
        // Безопасная инициализация с защитой от повторного вызова
        private async Task SafeInitializeAsync()
        {
            if (_isLoadingData) return;
            _isLoadingData = true;
            IsLoading = true;
            try
            {
                await LoadTasksAsync();
                await LoadFiltersAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка инициализации: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                _isLoadingData = false;
            }
        }
        #endregion
        #region Private Methods - Data Loading
        // Загрузить все задачи из базы данных
        private async Task LoadTasksAsync()
        {
            try
            {
                var tasks = await _taskRepository.GetAllAsync();
                Tasks.Clear();
                foreach (var task in tasks)
                    Tasks.Add(task);
                TasksView.View?.Refresh();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки задач: {ex.Message}";
            }
        }
        // Загрузить элементы для фильтров (теги, платформы, контесты)
        private async Task LoadFiltersAsync()
        {
            try
            {
                // Теги
                var tags = await _tagRepository.GetAllAsync();
                FilterTagItems.Clear();
                foreach (var tag in tags)
                {
                    var item = new FilterItem { Id = tag.Tag_ID, Name = tag.Название };
                    item.PropertyChanged += FilterItem_PropertyChanged;
                    FilterTagItems.Add(item);
                }
                OnPropertyChanged(nameof(SelectedTagsText));
                // Платформы
                var platforms = await _platformRepository.GetAllAsync();
                FilterPlatformItems.Clear();
                foreach (var platform in platforms)
                {
                    var item = new FilterItem { Id = platform.Platform_ID, Name = platform.Название };
                    item.PropertyChanged += FilterItem_PropertyChanged;
                    FilterPlatformItems.Add(item);
                }
                OnPropertyChanged(nameof(SelectedPlatformsText));
                // Контесты
                var contests = await _contestRepository.GetAllAsync();
                FilterContestItems.Clear();
                foreach (var contest in contests)
                {
                    var item = new FilterItem { Id = contest.Contest_ID, Name = contest.Название };
                    item.PropertyChanged += FilterItem_PropertyChanged;
                    FilterContestItems.Add(item);
                }
                OnPropertyChanged(nameof(SelectedContestsText));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки фильтров: {ex.Message}");
            }
        }
        #endregion
        #region Private Methods - Filtering
        // Обработчик изменения состояния чекбоксов в фильтрах
        private void FilterItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                ApplyFilters();
                OnPropertyChanged(nameof(SelectedTagsText));
                OnPropertyChanged(nameof(SelectedPlatformsText));
                OnPropertyChanged(nameof(SelectedContestsText));
            }
        }
        // Логика фильтрации задач (вызывается для каждой задачи при обновлении View)
        // Применяется OR логика: задача проходит, если соответствует хотя бы одному из выбранных критериев в каждой категории
        private void TasksView_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is ProgrammingTask task)
            {
                // Фильтр по диапазону сложности
                if (task.Сложность < MinDifficulty || task.Сложность > MaxDifficulty)
                {
                    e.Accepted = false;
                    return;
                }
                // Фильтр по тегам (OR логика: хотя бы один совпадает)
                var selectedTags = FilterTagItems.Where(x => x.IsSelected).Select(x => x.Id).ToList();
                bool hasAnyTag = selectedTags.Any() || ShowWithoutTag;
                if (hasAnyTag)
                {
                    bool matchTag = false;
                    if (selectedTags.Any() && selectedTags.Any(tagId => task.TagIds.Contains(tagId)))
                        matchTag = true;
                    if (ShowWithoutTag && !task.TagIds.Any())
                        matchTag = true;
                    if (!matchTag)
                    {
                        e.Accepted = false;
                        return;
                    }
                }
                // Фильтр по платформам (OR логика)
                var selectedPlatforms = FilterPlatformItems.Where(x => x.IsSelected).Select(x => x.Id).ToList();
                bool hasAnyPlatform = selectedPlatforms.Any() || ShowWithoutPlatform;
                if (hasAnyPlatform)
                {
                    bool matchPlatform = false;
                    if (selectedPlatforms.Any() && selectedPlatforms.Any(platformId => task.PlatformIds.Contains(platformId)))
                        matchPlatform = true;
                    if (ShowWithoutPlatform && !task.PlatformIds.Any())
                        matchPlatform = true;
                    if (!matchPlatform)
                    {
                        e.Accepted = false;
                        return;
                    }
                }
                // Фильтр по контестам (OR логика)
                var selectedContests = FilterContestItems.Where(x => x.IsSelected).Select(x => x.Id).ToList();
                bool hasAnyContest = selectedContests.Any() || ShowWithoutContest;
                if (hasAnyContest)
                {
                    bool matchContest = false;
                    if (selectedContests.Any() && selectedContests.Any(contestId => task.ContestIds.Contains(contestId)))
                        matchContest = true;
                    if (ShowWithoutContest && !task.ContestIds.Any())
                        matchContest = true;
                    if (!matchContest)
                    {
                        e.Accepted = false;
                        return;
                    }
                }
                e.Accepted = true;
            }
        }
        // Применить фильтры к коллекции задач (обновить View)
        private void ApplyFilters()
        {
            TasksView.View?.Refresh();
        }
        // Сбросить все фильтры к начальным значениям
        private void ClearFilters()
        {
            foreach (var item in FilterTagItems)
                item.IsSelected = false;
            foreach (var item in FilterPlatformItems)
                item.IsSelected = false;
            foreach (var item in FilterContestItems)
                item.IsSelected = false;
            ShowWithoutTag = false;
            ShowWithoutPlatform = false;
            ShowWithoutContest = false;
            MinDifficulty = 1;
            MaxDifficulty = 10;
        }
        #endregion
        #region Private Methods - UI Actions
        // Создать новую задачу через окно редактирования
        private void AddTask()
        {
            bool result = _navigationService.ShowTaskEditDialog();
            if (result)
                _ = LoadTasksAsync();
        }
        // Просмотреть задачу в отдельном окне (только чтение)
        private void ViewTask(ProgrammingTask task)
        {
            if (task == null)
            {
                _dialogService?.ShowError("Задача не выбрана");
                return;
            }
            try
            {
                var window = new Views.TaskViewWindow(task);
                var mainWindow = System.Windows.Application.Current.MainWindow;
                if (mainWindow != null && mainWindow != window)
                {
                    window.Owner = mainWindow;
                }
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                _dialogService?.ShowError($"Ошибка открытия окна просмотра:\n{ex.Message}");
            }
        }
        // Редактировать существующую задачу через окно редактирования
        private void EditTask(ProgrammingTask task)
        {
            if (task == null) return;
            bool result = _navigationService.ShowTaskEditDialog(task);
            if (result)
                _ = LoadTasksAsync();
        }
        // Удалить задачу с подтверждением пользователя
        private async Task DeleteTaskAsync(ProgrammingTask task)
        {
            if (task == null) return;
            string message = $"Удалить задачу \"{task.Название}\"?";
            if (!_dialogService.ShowConfirmation(message, "Подтверждение удаления"))
                return;
            IsLoading = true;
            try
            {
                bool success = await _taskRepository.DeleteAsync(task.Task_ID);
                if (success)
                {
                    Tasks.Remove(task);
                    if (SelectedTask == task)
                        SelectedTask = null;
                    _dialogService.ShowInfo("Задача успешно удалена!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось удалить задачу.");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка удаления: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion
    }
}