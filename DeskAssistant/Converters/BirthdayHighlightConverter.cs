using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace DeskAssistant.Converters
{
    public class BirthdayHighlightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isBirthdayToday)
            {
                if (isBirthdayToday)
                {
                    return new SolidColorBrush(Microsoft.UI.Colors.Yellow);
                }
                else
                {
                    return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                }
            }
            return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

}
