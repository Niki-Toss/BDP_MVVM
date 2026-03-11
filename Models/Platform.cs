namespace BDP_MVVM.Models
{
    // Модель платформы для автоматической проверки задач (Codeforces, Yandex Contest и т.д.)
    public class Platform
    {
        public int Platform_ID { get; set; }
        public string Название { get; set; }
        public bool Автопроверка_готовности { get; set; }
        // Заполняется в Repository через COUNT в SQL запросе
        public int КоличествоЗадач { get; set; }
        // Текстовое представление статуса для отображения в таблице
        public string АвтопроверкаТекст =>
            Автопроверка_готовности ? "✅ Есть автопроверка" : "❌ Нет автопроверки";
        // Для отображения в ComboBox при выборе платформы
        public override string ToString() => Название;
    }
}