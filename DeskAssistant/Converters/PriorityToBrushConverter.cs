using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace DeskAssistant.Converters
{
    public class PriorityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PrioritiesLevel priority)
            {
                switch (priority)
                {
                    case PrioritiesLevel.Высокий:
                        return new SolidColorBrush(Colors.Red);

                    case PrioritiesLevel.Средний:
                        return new SolidColorBrush(Colors.Orange);

                    case PrioritiesLevel.Низкий:
                        return new SolidColorBrush(Colors.YellowGreen);

                    default:
                        return new SolidColorBrush(Colors.Gray);
                }

            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
