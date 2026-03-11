using System;

namespace BDP_MVVM.Models
{
    // Модель пользователя системы (администраторы, обычные пользователи)
    public class User
    {
        public int User_ID { get; set; }
        public string Логин { get; set; }
        // SHA-256 хеш пароля (храним хеш, а не открытый текст)
        public string Пароль_hash { get; set; }
        public string Email { get; set; }
        public string Описание { get; set; }
        public int Role_ID { get; set; }
        public DateTime? Дата_создания { get; set; }
        // Объект роли с правами доступа (заполняется через JOIN)
        public Role Роль { get; set; }
        // Дата создания в удобном формате для отображения
        public string ДатаФорматированная => Дата_создания.HasValue
            ? Дата_создания.Value.ToString("dd.MM.yyyy")
            : "—";
        // Название роли для показа в таблице пользователей
        public string РольНазвание => Роль?.Название ?? "—";
        // Для отображения в списках и логах
        public override string ToString() => Логин;
    }
}