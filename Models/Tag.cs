namespace BDP_MVVM.Models
{
    // Модель тега задачи (тема/категория: Графы, ДП, Жадные алгоритмы и т.д.)
    public class Tag
    {
        public int Tag_ID { get; set; }
        public string Название { get; set; }
        // Вычисляется через COUNT в SQL запросе для статистики
        public int КоличествоЗадач { get; set; }
        // Для отображения в ComboBox и списках выбора
        public override string ToString() => Название;
    }
}