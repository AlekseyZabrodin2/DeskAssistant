using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace DeskAssistant.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string status)
            {
                switch (status?.ToLower())
                {
                    case "ожидание":
                        return new SolidColorBrush(Colors.Orange);

                    case "выполняется":
                        return new SolidColorBrush(Colors.Green);

                    case "завершено":
                        return new SolidColorBrush(Colors.LightSkyBlue);

                    case "просрочено":
                        return new SolidColorBrush(Colors.DarkRed);

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
