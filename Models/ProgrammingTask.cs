using System;
using System.Collections.Generic;

namespace BDP_MVVM.Models
{
    // Модель задачи по программированию из банка задач
    // Содержит условие, решение, связи с тегами, платформами и контестами
    public class ProgrammingTask
    {
        #region Primary Properties
        // Уникальный идентификатор задачи
        public int Task_ID { get; set; }
        // Название задачи
        public string Название { get; set; }
        // Уровень сложности задачи (от 1 до 10)
        public int Сложность { get; set; }
        // Краткое описание условия задачи
        public string Краткое_условие { get; set; }
        // Идея решения или подсказка
        public string Идея_решения { get; set; }
        // Ссылка на задачу в Polygon (Codeforces)
        public string Ссылка_polygon { get; set; }
        // Дополнительные примечания или комментарии
        public string Примечание { get; set; }
        // ID автора задачи
        public int? Автор_ID { get; set; }
        // Дата создания задачи в системе
        public DateTime? Дата_создания { get; set; }
        #endregion
        #region Navigation Properties
        // Автор задачи (загружается через JOIN)
        public User Автор { get; set; }
        #endregion
        #region Computed Properties
        // Дата создания в формате ДД.ММ.ГГГГ для отображения в таблицах
        public string ДатаСозданияФорматированная =>
            Дата_создания.HasValue ? Дата_создания.Value.ToString("dd.MM.yyyy") : "—";
        // Текстовое представление сложности с эмодзи для удобства пользователя
        public string СложностьТекст
        {
            get
            {
                return Сложность switch
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
            }
        }
        // Проверка готовности задачи для Codeforces (Platform_ID = 1)
        public bool Готовность_Codeforces => PlatformIds?.Contains(1) ?? false;
        #endregion
        #region Collection Properties
        // Список ID тегов, связанных с задачей (для фильтрации)
        public List<int> TagIds { get; set; } = new List<int>();
        // Список ID платформ, на которых размещена задача (для фильтрации)
        public List<int> PlatformIds { get; set; } = new List<int>();
        // Список ID контестов, в которых использовалась задача (для фильтрации)
        public List<int> ContestIds { get; set; } = new List<int>();
        // Полные объекты тегов, связанных с задачей
        public List<Tag> Теги { get; set; } = new List<Tag>();
        // Полные объекты платформ, на которых размещена задача
        public List<Platform> Платформы { get; set; } = new List<Platform>();
        // Полные объекты контестов, в которых использовалась задача
        public List<Contest> Контесты { get; set; } = new List<Contest>();
        #endregion
        #region Methods
        // Строковое представление задачи для отображения в ComboBox
        public override string ToString() => Название;
        #endregion
    }
}