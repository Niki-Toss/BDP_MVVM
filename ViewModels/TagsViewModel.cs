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
    // ViewModel для управления справочником тегов (тем и категорий задач)
    // Поддерживает CRUD операции и проверку использования тегов перед удалением
    public class TagsViewModel : ViewModelBase
    {
        #region Fields
        private readonly ITagRepository _tagRepository;
        private readonly IAuthenticationService _authService;
        private readonly IDialogService _dialogService;
        private Tag _selectedTag;
        private string _newTagName;
        private string _editTagName;
        private bool _isEditing;
        #endregion
        #region Properties
        // Коллекция тегов для отображения в UI
        public ObservableCollection<Tag> Tags { get; }
        // Выбранный тег в списке
        public Tag SelectedTag
        {
            get => _selectedTag;
            set
            {
                SetProperty(ref _selectedTag, value);
                if (value != null)
                {
                    EditTagName = value.Название;
                }
                IsEditing = false;
            }
        }
        // Название для создания нового тега
        public string NewTagName
        {
            get => _newTagName;
            set => SetProperty(ref _newTagName, value);
        }
        // Название для редактирования выбранного тега
        public string EditTagName
        {
            get => _editTagName;
            set => SetProperty(ref _editTagName, value);
        }
        // Включен ли режим редактирования
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }
        // Есть ли теги в коллекции
        public bool HasTags => Tags.Count > 0;
        // Может ли пользователь модифицировать теги (проверка прав)
        public bool CanModify => _authService?.CanEditTasks() ?? false;
        #endregion
        #region Commands
        // Команда загрузки тегов из базы данных
        public ICommand LoadTagsCommand { get; private set; }
        // Команда создания нового тега
        public ICommand AddTagCommand { get; private set; }
        // Команда перехода в режим редактирования
        public ICommand EditTagCommand { get; private set; }
        // Команда сохранения изменений тега
        public ICommand SaveEditCommand { get; private set; }
        // Команда отмены редактирования
        public ICommand CancelEditCommand { get; private set; }
        // Команда удаления тега
        public ICommand DeleteTagCommand { get; private set; }
        #endregion
        #region Constructor
        // Инициализация ViewModel для управления тегами
        public TagsViewModel(
            ITagRepository tagRepository,
            IAuthenticationService authService,
            IDialogService dialogService)
        {
            _tagRepository = tagRepository;
            _authService = authService;
            _dialogService = dialogService;
            Tags = new ObservableCollection<Tag>();
            // Инициализируем команды с проверкой прав доступа
            LoadTagsCommand = new AsyncRelayCommand(async _ => await LoadTagsAsync());
            AddTagCommand = new AsyncRelayCommand(
                async _ => await AddTagAsync(),
                _ => !string.IsNullOrWhiteSpace(NewTagName) && CanModify);
            EditTagCommand = new RelayCommand(
                _ => StartEdit(),
                _ => SelectedTag != null && CanModify);
            SaveEditCommand = new AsyncRelayCommand(
                async _ => await SaveEditAsync(),
                _ => !string.IsNullOrWhiteSpace(EditTagName) && IsEditing);
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            DeleteTagCommand = new AsyncRelayCommand(
                async _ => await DeleteTagAsync(),
                _ => SelectedTag != null && CanModify);
            // Загружаем данные при создании ViewModel
            _ = LoadTagsAsync();
        }
        #endregion
        #region Private Methods
        // Загрузить все теги из базы данных
        private async Task LoadTagsAsync()
        {
            IsLoading = true;
            try
            {
                var tags = await _tagRepository.GetAllAsync();
                Tags.Clear();
                foreach (var tag in tags)
                    Tags.Add(tag);
                OnPropertyChanged(nameof(HasTags));
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
        // Создать новый тег
        private async Task AddTagAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTagName)) return;
            IsLoading = true;
            try
            {
                var tag = new Tag { Название = NewTagName.Trim() };
                int newId = await _tagRepository.CreateAsync(tag);
                if (newId > 0)
                {
                    tag.Tag_ID = newId;
                    Tags.Add(tag);
                    // Очищаем поле формы
                    NewTagName = string.Empty;
                    OnPropertyChanged(nameof(HasTags));
                    _dialogService.ShowInfo($"Тег \"{tag.Название}\" успешно создан!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось создать тег.");
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
        // Перейти в режим редактирования выбранного тега
        private void StartEdit()
        {
            if (SelectedTag == null) return;
            EditTagName = SelectedTag.Название;
            IsEditing = true;
        }
        // Сохранить изменения тега в базу данных
        private async Task SaveEditAsync()
        {
            if (SelectedTag == null || string.IsNullOrWhiteSpace(EditTagName)) return;
            IsLoading = true;
            try
            {
                SelectedTag.Название = EditTagName.Trim();
                bool success = await _tagRepository.UpdateAsync(SelectedTag);
                if (success)
                {
                    // Обновляем элемент в коллекции для триггера UI
                    int index = Tags.IndexOf(SelectedTag);
                    if (index >= 0)
                    {
                        Tags[index] = SelectedTag;
                        SelectedTag = Tags[index];
                    }
                    IsEditing = false;
                    _dialogService.ShowInfo("Тег успешно обновлён!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось обновить тег.");
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
        // Отменить редактирование тега
        private void CancelEdit()
        {
            IsEditing = false;
            if (SelectedTag != null)
                EditTagName = SelectedTag.Название;
        }
        // Удалить выбранный тег с предупреждением о связанных задачах
        private async Task DeleteTagAsync()
        {
            if (SelectedTag == null) return;
            // Проверяем используется ли тег
            bool isUsed = await _tagRepository.IsUsedInTasksAsync(SelectedTag.Tag_ID);
            string message = isUsed
                ? $"Тег \"{SelectedTag.Название}\" используется в {SelectedTag.КоличествоЗадач} задачах.\n\nВсё равно удалить?"
                : $"Удалить тег \"{SelectedTag.Название}\"?";
            if (!_dialogService.ShowConfirmation(message, "Подтверждение удаления"))
                return;
            IsLoading = true;
            try
            {
                bool success = await _tagRepository.DeleteAsync(SelectedTag.Tag_ID);
                if (success)
                {
                    Tags.Remove(SelectedTag);
                    SelectedTag = null;
                    IsEditing = false;
                    OnPropertyChanged(nameof(HasTags));
                    _dialogService.ShowInfo("Тег успешно удалён!", "Успех");
                }
                else
                {
                    _dialogService.ShowError("Не удалось удалить тег.");
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