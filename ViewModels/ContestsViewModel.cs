using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using BDP_MVVM.Common;
using BDP_MVVM.Models;
using BDP_MVVM.Repositories.Interfaces;
using BDP_MVVM.Services.Interfaces;

namespace BDP_MVVM.ViewModels
{
    // ViewModel для управления справочником контестов
    // Поддерживает CRUD операции и группировку по годам
    public class ContestsViewModel : ViewModelBase
    {
        #region Fields
        private readonly IContestRepository _contestRepository;
        private readonly IAuthenticationService _authService;
        private readonly IDialogService _dialogService;
        private Contest _selectedContest;
        private bool _isEditing;
        private string _editНазвание;
        private int _editГод = DateTime.Now.Year;
        private string _newНазвание;
        private int _newГод = DateTime.Now.Year;
        #endregion
        #region Properties
        // Коллекция контестов для отображения в UI
        public ObservableCollection<Contest> Contests { get; }
        // View с группировкой контестов по годам и сортировкой
        public CollectionViewSource ContestsView { get; }
        // Выбранный контест в списке
        public Contest SelectedContest
        {
            get => _selectedContest;
            set
            {
                SetProperty(ref _selectedContest, value);
                if (value != null)
                {
                    EditНазвание = value.Название;
                    EditГод = value.Год_создания;
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
        // Есть ли контесты в коллекции
        public bool HasContests => Contests.Count > 0;
        // Может ли пользователь модифицировать контесты (проверка прав)
        public bool CanModify => _authService?.CanEditTasks() ?? false;
        // Название для создания нового контеста
        public string NewНазвание
        {
            get => _newНазвание;
            set => SetProperty(ref _newНазвание, value);
        }
        // Год для создания нового контеста
        public int NewГод
        {
            get => _newГод;
            set => SetProperty(ref _newГод, value);
        }
        // Название для редактирования выбранного контеста
        public string EditНазвание
        {
            get => _editНазвание;
            set => SetProperty(ref _editНазвание, value);
        }
        // Год для редактирования выбранного контеста
        public int EditГод
        {
            get => _editГод;
            set => SetProperty(ref _editГод, value);
        }
        #endregion
        #region Commands
        // Команда загрузки контестов из базы данных
        public ICommand LoadCommand { get; private set; }
        // Команда создания нового контеста
        public ICommand AddCommand { get; private set; }
        // Команда перехода в режим редактирования
        public ICommand EditCommand { get; private set; }
        // Команда сохранения изменений контеста
        public ICommand SaveEditCommand { get; private set; }
        // Команда отмены редактирования
        public ICommand CancelEditCommand { get; private set; }
        // Команда удаления контеста
        public ICommand DeleteCommand { get; private set; }
        #endregion
        #region Constructor
        // Инициализация ViewModel для управления контестами
        public ContestsViewModel(
            IContestRepository contestRepository,
            IAuthenticationService authService,
            IDialogService dialogService)
        {
            _contestRepository = contestRepository;
            _authService = authService;
            _dialogService = dialogService;
            Contests = new ObservableCollection<Contest>();
            // Настраиваем группировку по годам и сортировку
            ContestsView = new CollectionViewSource { Source = Contests };
            ContestsView.GroupDescriptions.Add(new PropertyGroupDescription("Год"));
            ContestsView.SortDescriptions.Add(
                new SortDescription("Год_создания", ListSortDirection.Descending));
            // Инициализируем команды с проверкой прав доступа
            LoadCommand = new AsyncRelayCommand(async _ => await LoadAsync());
            AddCommand = new AsyncRelayCommand(
                async _ => await AddAsync(),
                _ => !string.IsNullOrWhiteSpace(NewНазвание) && NewГод > 2000 && CanModify);
            EditCommand = new RelayCommand(
                _ => StartEdit(),
                _ => SelectedContest != null && CanModify);
            SaveEditCommand = new AsyncRelayCommand(
                async _ => await SaveEditAsync(),
                _ => !string.IsNullOrWhiteSpace(EditНазвание) && EditГод > 2000 && IsEditing);
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            DeleteCommand = new AsyncRelayCommand(
                async _ => await DeleteAsync(),
                _ => SelectedContest != null && CanModify);
            // Загружаем данные при создании ViewModel
            _ = LoadAsync();
        }
        #endregion
        #region Private Methods
        // Загрузить все контесты из базы данных
        private async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var contests = await _contestRepository.GetAllAsync();
                Contests.Clear();
                foreach (var c in contests)
                    Contests.Add(c);
                ContestsView.View.Refresh();
                OnPropertyChanged(nameof(HasContests));
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
        // Создать новый контест
        private async Task AddAsync()
        {
            IsLoading = true;
            try
            {
                var contest = new Contest
                {
                    Название = NewНазвание.Trim(),
                    Год_создания = NewГод
                };
                int newId = await _contestRepository.CreateAsync(contest);
                if (newId > 0)
                {
                    contest.Contest_ID = newId;
                    Contests.Add(contest);
                    ContestsView.View.Refresh();
                    // Очищаем поля формы
                    NewНазвание = string.Empty;
                    NewГод = DateTime.Now.Year;
                    OnPropertyChanged(nameof(HasContests));
                    _dialogService.ShowInfo($"Контест \"{contest.Название}\" создан!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось создать контест.");
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
        // Перейти в режим редактирования выбранного контеста
        private void StartEdit()
        {
            if (SelectedContest == null) return;
            EditНазвание = SelectedContest.Название;
            EditГод = SelectedContest.Год_создания;
            IsEditing = true;
        }
        // Сохранить изменения контеста в базу данных
        private async Task SaveEditAsync()
        {
            if (SelectedContest == null) return;
            IsLoading = true;
            try
            {
                SelectedContest.Название = EditНазвание.Trim();
                SelectedContest.Год_создания = EditГод;
                bool success = await _contestRepository.UpdateAsync(SelectedContest);
                if (success)
                {
                    // Обновляем элемент в коллекции для триггера UI
                    int index = Contests.IndexOf(SelectedContest);
                    if (index >= 0)
                    {
                        Contests[index] = SelectedContest;
                        SelectedContest = Contests[index];
                    }
                    ContestsView.View.Refresh();
                    IsEditing = false;
                    _dialogService.ShowInfo("Контест успешно обновлён!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось обновить контест.");
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
        // Отменить редактирование контеста
        private void CancelEdit()
        {
            IsEditing = false;
        }
        // Удалить выбранный контест с предупреждением о связанных задачах
        private async Task DeleteAsync()
        {
            if (SelectedContest == null) return;
            string message = SelectedContest.КоличествоЗадач > 0
                ? $"Контест \"{SelectedContest.Название}\" содержит {SelectedContest.КоличествоЗадач} задач.\n\nВсё равно удалить?"
                : $"Удалить контест \"{SelectedContest.Название}\"?";
            if (!_dialogService.ShowConfirmation(message, "Подтверждение"))
                return;
            IsLoading = true;
            try
            {
                bool success = await _contestRepository.DeleteAsync(SelectedContest.Contest_ID);
                if (success)
                {
                    Contests.Remove(SelectedContest);
                    ContestsView.View.Refresh();
                    SelectedContest = null;
                    IsEditing = false;
                    OnPropertyChanged(nameof(HasContests));
                    _dialogService.ShowInfo("Контест удалён!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось удалить контест.");
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