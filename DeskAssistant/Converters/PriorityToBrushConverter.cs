using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace DeskAssistant.Converters
{
    public class PriorityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PrioritiesLevelEnum priority)
            {
                switch (priority)
                {
                    case PrioritiesLevelEnum.Высокий:
                        return new SolidColorBrush(Colors.Red);

                    case PrioritiesLevelEnum.Средний:
                        return new SolidColorBrush(Colors.Orange);

                    case PrioritiesLevelEnum.Низкий:
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
