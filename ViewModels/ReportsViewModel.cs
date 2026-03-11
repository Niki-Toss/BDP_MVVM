using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BDP_MVVM.Common;
using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;
using BDP_MVVM.Services.Interfaces;

namespace BDP_MVVM.ViewModels
{
    // ViewModel страницы отчётов и аналитики
    // Отображает общую статистику по банку задач и поддерживает экспорт данных в CSV
    public class ReportsViewModel : ViewModelBase
    {
        #region Fields
        private readonly ITaskRepository _taskRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IContestRepository _contestRepository;
        private readonly IDialogService _dialogService;
        private int _totalTasks;
        private int _totalTags;
        private int _totalContests;
        private int _readyTasks;
        private double _avgDifficulty;
        #endregion
        #region Properties
        // Общее количество задач в банке
        public int TotalTasks
        {
            get => _totalTasks;
            set => SetProperty(ref _totalTasks, value);
        }
        // Общее количество тегов в системе
        public int TotalTags
        {
            get => _totalTags;
            set => SetProperty(ref _totalTags, value);
        }
        // Общее количество контестов в системе
        public int TotalContests
        {
            get => _totalContests;
            set => SetProperty(ref _totalContests, value);
        }
        // Количество задач готовых для Codeforces
        public int ReadyTasks
        {
            get => _readyTasks;
            set => SetProperty(ref _readyTasks, value);
        }
        // Средняя сложность задач (от 1 до 10)
        public double AvgDifficulty
        {
            get => _avgDifficulty;
            set => SetProperty(ref _avgDifficulty, value);
        }
        // Средняя сложность в текстовом формате для отображения
        public string AvgDifficultyText => AvgDifficulty > 0 ? $"{AvgDifficulty:F1} / 10" : "—";
        // Процент готовых задач для Codeforces
        public string ReadyTasksPercent => TotalTasks > 0 ? $"{(ReadyTasks * 100 / TotalTasks)}%" : "0%";
        // Распределение задач по уровням сложности для отображения в диаграмме
        public ObservableCollection<DifficultyGroup> DifficultyGroups { get; }
        // Список всех задач для таблицы и экспорта
        public ObservableCollection<ProgrammingTask> Tasks { get; }
        #endregion
        #region Commands
        // Команда загрузки данных и расчёта статистики
        public ICommand LoadCommand { get; private set; }
        // Команда экспорта списка задач в CSV файл
        public ICommand ExportTasksCsvCommand { get; private set; }
        #endregion
        #region Constructor
        // Инициализация ViewModel для страницы отчётов
        public ReportsViewModel(
            ITaskRepository taskRepository,
            ITagRepository tagRepository,
            IContestRepository contestRepository,
            IDialogService dialogService)
        {
            _taskRepository = taskRepository;
            _tagRepository = tagRepository;
            _contestRepository = contestRepository;
            _dialogService = dialogService;
            DifficultyGroups = new ObservableCollection<DifficultyGroup>();
            Tasks = new ObservableCollection<ProgrammingTask>();
            LoadCommand = new AsyncRelayCommand(async _ => await LoadAsync());
            ExportTasksCsvCommand = new AsyncRelayCommand(async _ => await ExportTasksCsvAsync());
            // Загружаем данные при открытии страницы
            _ = LoadAsync();
        }
        #endregion
        #region Private Methods
        // Загрузить все данные и вычислить статистику
        private async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                // Загружаем данные параллельно для скорости
                var tasksTask = _taskRepository.GetAllAsync();
                var tagsTask = _tagRepository.GetAllAsync();
                var contestsTask = _contestRepository.GetAllAsync();
                await Task.WhenAll(tasksTask, tagsTask, contestsTask);
                var tasks = tasksTask.Result;
                var tags = tagsTask.Result;
                var contests = contestsTask.Result;
                // Подсчитываем общую статистику
                TotalTasks = tasks.Count;
                TotalTags = tags.Count;
                TotalContests = contests.Count;
                ReadyTasks = tasks.Count(t => t.Готовность_Codeforces);
                AvgDifficulty = tasks.Count > 0 ? tasks.Average(t => t.Сложность) : 0;
                OnPropertyChanged(nameof(AvgDifficultyText));
                OnPropertyChanged(nameof(ReadyTasksPercent));
                // Группируем задачи по уровням сложности
                DifficultyGroups.Clear();
                var groups = new[]
                {
                    new DifficultyGroup
                    {
                        Название = "😊 Лёгкие (1-2)",
                        Диапазон = "1-2",
                        Количество = tasks.Count(t => t.Сложность <= 2),
                        Цвет = "#8BC34A"
                    },
                    new DifficultyGroup
                    {
                        Название = "🙂 Ниже среднего (3-4)",
                        Диапазон = "3-4",
                        Количество = tasks.Count(t => t.Сложность >= 3 && t.Сложность <= 4),
                        Цвет = "#FFC107"
                    },
                    new DifficultyGroup
                    {
                        Название = "😐 Средние (5-6)",
                        Диапазон = "5-6",
                        Количество = tasks.Count(t => t.Сложность >= 5 && t.Сложность <= 6),
                        Цвет = "#FF9800"
                    },
                    new DifficultyGroup
                    {
                        Название = "😰 Сложные (7-8)",
                        Диапазон = "7-8",
                        Количество = tasks.Count(t => t.Сложность >= 7 && t.Сложность <= 8),
                        Цвет = "#F44336"
                    },
                    new DifficultyGroup
                    {
                        Название = "💀 Очень сложные (9-10)",
                        Диапазон = "9-10",
                        Количество = tasks.Count(t => t.Сложность >= 9),
                        Цвет = "#9C27B0"
                    },
                };
                // Считаем проценты для каждой группы
                foreach (var g in groups)
                {
                    g.Процент = TotalTasks > 0 ? (double)g.Количество / TotalTasks * 100 : 0;
                    DifficultyGroups.Add(g);
                }
                // Заполняем таблицу задач (сортируем по сложности)
                Tasks.Clear();
                foreach (var task in tasks.OrderBy(t => t.Сложность))
                    Tasks.Add(task);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        // Экспортировать список задач в CSV файл
        private async Task ExportTasksCsvAsync()
        {
            try
            {
                string path = _dialogService.ShowSaveFileDialog(
                    "CSV файлы (*.csv)|*.csv",
                    $"tasks_export_{DateTime.Now:yyyyMMdd}");
                if (string.IsNullOrEmpty(path)) return;
                IsLoading = true;
                var sb = new StringBuilder();
                // Формируем CSV (разделитель - точка с запятой для Excel)
                sb.AppendLine("ID;Название;Сложность;Готовность CF;Автор;Дата создания;Краткое условие");
                foreach (var task in Tasks)
                {
                    sb.AppendLine(string.Join(";",
                        task.Task_ID,
                        $"\"{task.Название}\"",
                        task.Сложность,
                        task.Готовность_Codeforces ? "Да" : "Нет",
                        $"\"{task.Автор?.Логин ?? "—"}\"",
                        task.ДатаСозданияФорматированная,
                        $"\"{task.Краткое_условие?.Replace("\"", "'") ?? ""}\""));
                }
                // Сохраняем в файл
                await Task.Run(() => File.WriteAllText(path, sb.ToString(), Encoding.UTF8));
                _dialogService.ShowInfo($"Экспорт завершён!\n\nФайл сохранён:\n{path}", "Экспорт CSV");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка экспорта: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion
    }
    // Группа задач по уровню сложности для отображения в диаграмме
    public class DifficultyGroup
    {
        #region Properties
        // Название группы с эмодзи (например, "😊 Лёгкие (1-2)")
        public string Название { get; set; }
        // Диапазон уровней сложности (например, "1-2")
        public string Диапазон { get; set; }
        // Количество задач в данной группе
        public int Количество { get; set; }
        // Процент задач от общего количества
        public double Процент { get; set; }
        // Цвет для отображения группы в диаграмме (HEX формат)
        public string Цвет { get; set; }
        // Процент в текстовом формате для отображения
        public string ПроцентТекст => $"{Процент:F0}%";
        #endregion
    }
}