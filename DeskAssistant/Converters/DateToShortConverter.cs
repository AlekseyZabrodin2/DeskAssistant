using Microsoft.UI.Xaml.Data;

namespace DeskAssistant.Converters
{
    public class DateToShortConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime date)
            {
                return date.ToString("dd.MM");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (DateTime.TryParseExact(value.ToString(), "dd.MM", null, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return new DateTime(2000, result.Month, result.Day);
            }
            return DateTime.MinValue;
        }
    }
}
