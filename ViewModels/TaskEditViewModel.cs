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
    // ViewModel окна редактирования задачи
    // Поддерживает создание новой задачи и изменение существующей
    // Управляет полями задачи и связями с тегами, платформами, контестами
    public class TaskEditViewModel : ViewModelBase
    {
        #region Fields
        private readonly ITaskRepository _taskRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IPlatformRepository _platformRepository;
        private readonly IContestRepository _contestRepository;
        private readonly IDialogService _dialogService;
        private readonly ProgrammingTask _originalTask;
        private readonly bool _isNewTask;
        private string _название;
        private int _сложность = 5;
        private string _краткоеУсловие;
        private string _идеяРешения;
        private string _ссылкаPolygon;
        private string _примечание;
        #endregion
        #region Properties
        // Название задачи
        public string Название
        {
            get => _название;
            set => SetProperty(ref _название, value);
        }
        // Уровень сложности задачи (от 1 до 10)
        public int Сложность
        {
            get => _сложность;
            set
            {
                SetProperty(ref _сложность, value);
                OnPropertyChanged(nameof(СложностьТекст));
            }
        }
        // Текстовое представление уровня сложности с эмодзи
        public string СложностьТекст => Сложность switch
        {
            1 => "😊 Очень лёгкая",
            2 => "🙂 Лёгкая",
            3 => "😐 Ниже среднего",
            4 => "🤔 Средняя",
            5 => "😅 Выше среднего",
            6 => "😰 Сложная",
            7 => "😱 Очень сложная",
            8 => "🔥 Экстремальная",
            9 => "💀 Почти невозможная",
            10 => "☠️ Невозможная",
            _ => "—"
        };
        // Краткое условие задачи
        public string КраткоеУсловие
        {
            get => _краткоеУсловие;
            set => SetProperty(ref _краткоеУсловие, value);
        }
        // Идея решения или подсказка
        public string ИдеяРешения
        {
            get => _идеяРешения;
            set => SetProperty(ref _идеяРешения, value);
        }
        // Ссылка на задачу в Polygon (Codeforces)
        public string СсылкаPolygon
        {
            get => _ссылкаPolygon;
            set => SetProperty(ref _ссылкаPolygon, value);
        }
        // Дополнительные примечания к задаче
        public string Примечание
        {
            get => _примечание;
            set => SetProperty(ref _примечание, value);
        }
        // Заголовок окна (меняется в зависимости от режима)
        public string WindowTitle => _isNewTask ? "Новая задача" : "Редактирование задачи";
        // Текст кнопки сохранения (меняется в зависимости от режима)
        public string SaveButtonText => _isNewTask ? "➕ Создать" : "💾 Сохранить";
        // Коллекция тегов с чекбоксами для выбора
        public ObservableCollection<TagCheckItem> Tags { get; } = new ObservableCollection<TagCheckItem>();
        // Коллекция платформ с чекбоксами и флагами готовности
        public ObservableCollection<PlatformCheckItem> Platforms { get; } = new ObservableCollection<PlatformCheckItem>();
        // Коллекция контестов с чекбоксами для выбора
        public ObservableCollection<ContestCheckItem> Contests { get; } = new ObservableCollection<ContestCheckItem>();
        #endregion
        #region Commands
        // Команда сохранения задачи (создание или обновление)
        public ICommand SaveCommand { get; private set; }
        // Команда отмены редактирования и закрытия окна
        public ICommand CancelCommand { get; private set; }
        // Команда увеличения уровня сложности на 1
        public ICommand IncreaseDifficultyCommand { get; private set; }
        // Команда уменьшения уровня сложности на 1
        public ICommand DecreaseDifficultyCommand { get; private set; }
        #endregion
        #region Events
        // Событие запроса закрытия окна
        // Параметр: true = сохранено успешно, false = отменено
        public event Action<bool> CloseRequested;
        #endregion
        #region Constructors
        // Инициализация ViewModel для создания новой задачи
        public TaskEditViewModel(
            ITaskRepository taskRepository,
            ITagRepository tagRepository,
            IPlatformRepository platformRepository,
            IContestRepository contestRepository,
            IDialogService dialogService)
        {
            _taskRepository = taskRepository;
            _tagRepository = tagRepository;
            _platformRepository = platformRepository;
            _contestRepository = contestRepository;
            _dialogService = dialogService;
            _isNewTask = true;
            InitCommands();
            _ = LoadRelatedDataAsync(null);
        }
        // Инициализация ViewModel для редактирования существующей задачи
        public TaskEditViewModel(
            ProgrammingTask task,
            ITaskRepository taskRepository,
            ITagRepository tagRepository,
            IPlatformRepository platformRepository,
            IContestRepository contestRepository,
            IDialogService dialogService)
        {
            _taskRepository = taskRepository;
            _tagRepository = tagRepository;
            _platformRepository = platformRepository;
            _contestRepository = contestRepository;
            _dialogService = dialogService;
            _originalTask = task;
            _isNewTask = false;
            // Заполняем поля данными существующей задачи
            Название = task.Название;
            Сложность = task.Сложность;
            КраткоеУсловие = task.Краткое_условие;
            ИдеяРешения = task.Идея_решения;
            СсылкаPolygon = task.Ссылка_polygon;
            Примечание = task.Примечание;
            InitCommands();
            _ = LoadRelatedDataAsync(task.Task_ID);
        }
        #endregion
        #region Private Methods
        // Инициализировать команды с условиями доступности
        private void InitCommands()
        {
            SaveCommand = new AsyncRelayCommand(
                async _ => await SaveAsync(),
                _ => !string.IsNullOrWhiteSpace(Название));
            CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke(false));
            IncreaseDifficultyCommand = new RelayCommand(
                _ => Сложность++,
                _ => Сложность < 10);
            DecreaseDifficultyCommand = new RelayCommand(
                _ => Сложность--,
                _ => Сложность > 1);
        }
        // Загрузить доступные теги, платформы и контесты
        // Отметить уже выбранные для существующей задачи
        private async Task LoadRelatedDataAsync(int? taskId)
        {
            IsLoading = true;
            try
            {
                // Загружаем теги и отмечаем выбранные
                var allTags = await _tagRepository.GetAllAsync();
                var selectedTagIds = taskId.HasValue
                    ? await _tagRepository.GetTagIdsByTaskAsync(taskId.Value)
                    : new System.Collections.Generic.List<int>();
                Tags.Clear();
                foreach (var tag in allTags)
                    Tags.Add(new TagCheckItem
                    {
                        Tag = tag,
                        IsSelected = selectedTagIds.Contains(tag.Tag_ID)
                    });
                // Загружаем платформы с флагами готовности
                var allPlatforms = await _platformRepository.GetAllAsync();
                var selectedPlatforms = taskId.HasValue
                    ? await _platformRepository.GetPlatformsByTaskAsync(taskId.Value)
                    : new System.Collections.Generic.List<TaskPlatformItem>();
                Platforms.Clear();
                foreach (var platform in allPlatforms)
                {
                    var existing = selectedPlatforms.FirstOrDefault(p => p.PlatformId == platform.Platform_ID);
                    Platforms.Add(new PlatformCheckItem
                    {
                        Platform = platform,
                        IsSelected = existing != null,
                        Готовность = existing?.Готовность ?? false
                    });
                }
                // Загружаем контесты и отмечаем выбранные
                var allContests = await _contestRepository.GetAllAsync();
                var selectedContestIds = taskId.HasValue
                    ? await _contestRepository.GetContestIdsByTaskAsync(taskId.Value)
                    : new System.Collections.Generic.List<int>();
                Contests.Clear();
                foreach (var contest in allContests)
                    Contests.Add(new ContestCheckItem
                    {
                        Contest = contest,
                        IsSelected = selectedContestIds.Contains(contest.Contest_ID)
                    });
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
        // Сохранить задачу со всеми связями (теги, платформы, контесты)
        private async Task SaveAsync()
        {
            IsLoading = true;
            try
            {
                int taskId;
                if (_isNewTask)
                {
                    // Создаём новую задачу
                    var task = new ProgrammingTask
                    {
                        Название = Название.Trim(),
                        Сложность = Сложность,
                        Краткое_условие = КраткоеУсловие?.Trim(),
                        Идея_решения = ИдеяРешения?.Trim(),
                        Ссылка_polygon = СсылкаPolygon?.Trim(),
                        Примечание = Примечание?.Trim()
                    };
                    taskId = await _taskRepository.CreateAsync(task);
                    if (taskId <= 0)
                    {
                        _dialogService.ShowError("Не удалось создать задачу.");
                        return;
                    }
                }
                else
                {
                    // Обновляем существующую задачу
                    _originalTask.Название = Название.Trim();
                    _originalTask.Сложность = Сложность;
                    _originalTask.Краткое_условие = КраткоеУсловие?.Trim();
                    _originalTask.Идея_решения = ИдеяРешения?.Trim();
                    _originalTask.Ссылка_polygon = СсылкаPolygon?.Trim();
                    _originalTask.Примечание = Примечание?.Trim();
                    bool updated = await _taskRepository.UpdateAsync(_originalTask);
                    if (!updated)
                    {
                        _dialogService.ShowError("Не удалось обновить задачу.");
                        return;
                    }
                    taskId = _originalTask.Task_ID;
                }
                // Сохраняем все связи через соответствующие репозитории
                // Теги
                var selectedTagIds = Tags.Where(t => t.IsSelected).Select(t => t.Tag.Tag_ID);
                await _tagRepository.SaveTaskTagsAsync(taskId, selectedTagIds);
                // Платформы с флагами готовности
                var selectedPlatforms = Platforms
                    .Where(p => p.IsSelected)
                    .Select(p => new TaskPlatformItem
                    {
                        PlatformId = p.Platform.Platform_ID,
                        Готовность = p.Готовность
                    });
                await _platformRepository.SaveTaskPlatformsAsync(taskId, selectedPlatforms);
                // Контесты
                var selectedContestIds = Contests.Where(c => c.IsSelected).Select(c => c.Contest.Contest_ID);
                await _contestRepository.SaveTaskContestsAsync(taskId, selectedContestIds);
                // Закрываем окно с успешным результатом
                CloseRequested?.Invoke(true);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка сохранения: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion
    }
}