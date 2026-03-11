using BDP_MVVM.Common;

namespace BDP_MVVM.Models
{
    #region Base Class
    // Базовый класс для элементов с возможностью выбора через чекбокс
    // Используется в окне редактирования задачи для выбора тегов, платформ и контестов
    public abstract class CheckItemBase<T> : ObservableObject
    {
        private bool _isSelected;
        // Элемент данных (тег, платформа или контест)
        public T Item { get; set; }
        // Отмечен ли элемент галочкой (выбран пользователем)
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
    #endregion
    #region Concrete Implementations
    // Элемент списка тегов с возможностью выбора через чекбокс
    // Используется в окне редактирования задачи
    public class TagCheckItem : CheckItemBase<Tag>
    {
        // Тег для отображения и выбора
        public Tag Tag
        {
            get => Item;
            set => Item = value;
        }
    }
    // Элемент списка платформ с возможностью выбора и указания готовности задачи
    // Используется в окне редактирования задачи
    public class PlatformCheckItem : CheckItemBase<Platform>
    {
        private bool _готовность;
        // Платформа для отображения и выбора
        public Platform Platform
        {
            get => Item;
            set => Item = value;
        }
        // Готовность задачи к размещению на данной платформе
        public bool Готовность
        {
            get => _готовность;
            set => SetProperty(ref _готовность, value);
        }
    }
    // Элемент списка контестов с возможностью выбора через чекбокс
    // Используется в окне редактирования задачи
    public class ContestCheckItem : CheckItemBase<Contest>
    {
        // Контест для отображения и выбора
        public Contest Contest
        {
            get => Item;
            set => Item = value;
        }
    }
    #endregion
}