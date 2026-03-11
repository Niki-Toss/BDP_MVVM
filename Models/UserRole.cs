namespace BDP_MVVM.Models
{
    // Модель роли пользователя с набором прав доступа
    public class Role
    {
        public int Role_ID { get; set; }
        // Системный код роли (admin, user, guest)
        public string Код_роли { get; set; }
        // Название для отображения пользователю (Администратор, Пользователь, Гость)
        public string Название { get; set; }
        // Права доступа - определяют что может делать пользователь с этой ролью
        public bool Может_редактировать_задачи { get; set; }
        public bool Может_удалять_задачи { get; set; }
        public bool Может_управлять_пользователями { get; set; }
        // Для отображения в ComboBox при назначении роли
        public override string ToString() => Название;
    }
}