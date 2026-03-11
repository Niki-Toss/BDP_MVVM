using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BDP_MVVM.Common
{
    // Базовый класс для моделей и VM. Автоматически уведомляет UI об изменениях свойств.
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // Вызывается когда свойство изменилось, то обновляет интерфейс.
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // Устанавливает новое значение свойства и уведомляет UI если оно реально изменилось.
        // Возвращает true если значение изменилось, false если осталось прежним.
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // Не обновляем если значение не изменилось (экономим ресурсы)
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}