namespace BDP_MVVM.Models
{
    // Модель контеста (соревнования по программированию)
    // Хранит информацию о соревнованиях и связях с задачами
    public class Contest
    {
        #region Primary Properties
        // Уникальный идентификатор контеста
        public int Contest_ID { get; set; }
        // Название контеста (например, "Codeforces Round #800")
        public string Название { get; set; }
        // Год проведения контеста
        public int Год_создания { get; set; }
        #endregion
        #region Computed Properties
        // Количество задач, связанных с данным контестом
        // Вычисляется в Repository при загрузке списка контестов
        public int КоличествоЗадач { get; set; }
        // Год проведения контеста в виде строки для группировки в отчётах
        // Возвращает "Год не указан" если год равен 0 или не задан
        public string Год => Год_создания > 0
            ? Год_создания.ToString()
            : "Год не указан";
        #endregion
        #region Methods
        // Строковое представление контеста для отображения в ComboBox и списках
        public override string ToString() => Название;
        #endregion
    }
}