using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BDP_MVVM.Views.Converters
{
    #region Visibility Converters
    // Преобразует bool в Visibility
    // true → Visible, false → Collapsed
    public class BooleanToVisibilityConverter : IValueConverter
    {
        // Преобразует булево значение в видимость элемента
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }
        // Обратное преобразование видимости в булево значение
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v == Visibility.Visible;
            return false;
        }
    }
    // Инверсия BooleanToVisibilityConverter
    // false → Visible, true → Collapsed
    // Используется для скрытия элементов при активном состоянии
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        // Преобразует булево значение в видимость с инверсией
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }
        // Обратное преобразование не поддерживается
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    // Показывает элемент только если строка не пустая
    // Используется для условного отображения текстовых блоков
    public class StringToVisibilityConverter : IValueConverter
    {
        // Преобразует строку в видимость (пустая строка → Collapsed)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        // Обратное преобразование не поддерживается
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    // Показывает элемент только если объект null
    // Используется для отображения placeholder текста в формах
    public class NullToVisibilityConverter : IValueConverter
    {
        // Преобразует null в Visible, остальное в Collapsed
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null ? Visibility.Visible : Visibility.Collapsed;
        // Обратное преобразование не поддерживается
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    // Показывает элемент только если объект не null
    // Используется для условного отображения контента
    public class NotNullToVisibilityConverter : IValueConverter
    {
        // Преобразует не-null в Visible, null в Collapsed
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null ? Visibility.Visible : Visibility.Collapsed;
        // Обратное преобразование не поддерживается
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    #endregion
    #region Task-related Converters
    // Преобразует уровень сложности задачи (1-10) в цвет
    // Цветовая схема: Зелёный (1-2) → Жёлтый (3-4) → Оранжевый (5-6) → Красный (7-8) → Фиолетовый (9-10)
    // Используется для визуализации сложности в списках задач
    public class DifficultyToColorConverter : IValueConverter
    {
        // Преобразует уровень сложности в цветную кисть
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int difficulty)
            {
                return difficulty switch
                {
                    <= 2 => new SolidColorBrush(Color.FromRgb(139, 195, 74)),   // Зелёный
                    <= 4 => new SolidColorBrush(Color.FromRgb(255, 193, 7)),    // Жёлтый
                    <= 6 => new SolidColorBrush(Color.FromRgb(255, 152, 0)),    // Оранжевый
                    <= 8 => new SolidColorBrush(Color.FromRgb(244, 67, 54)),    // Красный
                    _ => new SolidColorBrush(Color.FromRgb(156, 39, 176))       // Фиолетовый
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }
        // Обратное преобразование не поддерживается
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    // Преобразует готовность задачи в цвет
    // true (готово) → зелёный, false (не готово) → красный
    // Используется для индикации статуса готовности задачи для Codeforces
    public class ReadinessToColorConverter : IValueConverter
    {
        // Преобразует булево значение готовности в цветную кисть
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isReady)
                return isReady
                    ? new SolidColorBrush(Color.FromRgb(76, 175, 80))   // Зелёный
                    : new SolidColorBrush(Color.FromRgb(244, 67, 54));  // Красный
            return new SolidColorBrush(Colors.Gray);
        }
        // Обратное преобразование не поддерживается
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    // Преобразует готовность задачи в текст с иконкой
    // true → "✓ Готово", false → "✗ Не готово"
    // Используется для текстового отображения статуса готовности
    public class ReadinessToTextConverter : IValueConverter
    {
        // Преобразует булево значение готовности в текст с иконкой
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isReady)
                return isReady ? "✓ Готово" : "✗ Не готово";
            return "✗ Не готово";
        }
        // Обратное преобразование не поддерживается
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    #endregion
    #region Utility Converters
    // Инвертирует булево значение
    // true → false, false → true
    // Используется для биндинга к элементам с обратной логикой
    public class InverseBooleanConverter : IValueConverter
    {
        // Инвертирует булево значение
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return true;
        }
        // Обратное преобразование (также инверсия)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return false;
        }
    }
    // Преобразует процент (0-100) в ширину для визуализации (0-300px)
    // Используется в диаграммах на странице отчётов для отображения процентного соотношения
    public class PercentToWidthConverter : IValueConverter
    {
        // Преобразует процент в ширину пикселей (макс. 300px)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
                return Math.Max(0, Math.Min(300, percent * 3));
            return 0.0;
        }
        // Обратное преобразование не поддерживается
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    // Вычисляет отступы для визуализации диапазона сложности на двойном ползунке
    // Принимает массив значений: [min (int), max (int), width (double)]
    // Возвращает Thickness с рассчитанными левым и правым отступами для Border
    // Используется в фильтре задач для визуального отображения выбранного диапазона
    public class RangeMarginConverter : IMultiValueConverter
    {
        // Преобразует минимум, максимум и ширину контейнера в отступы для визуализации диапазона
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3 ||
                !(values[0] is int min) ||
                !(values[1] is int max) ||
                !(values[2] is double width))
            {
                return new Thickness(10, 0, 10, 0);
            }
            const int sliderMin = 1;
            const int sliderMax = 10;
            const double thumbWidth = 10;
            double range = sliderMax - sliderMin;
            double leftOffset = ((min - sliderMin) / range) * (width - 20) + thumbWidth;
            double rightOffset = ((sliderMax - max) / range) * (width - 20) + thumbWidth;
            return new Thickness(leftOffset, 0, rightOffset, 0);
        }
        // Обратное преобразование не поддерживается
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}