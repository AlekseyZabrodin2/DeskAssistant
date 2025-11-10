using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace DeskAssistant.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TaskStatusEnum status)
            {
                switch (status)
                {
                    case TaskStatusEnum.Pending:
                        return new SolidColorBrush(Colors.Orange);

                    case TaskStatusEnum.InProgress:
                        return new SolidColorBrush(Colors.Green);

                    case TaskStatusEnum.Completed:
                        return new SolidColorBrush(Colors.LightSkyBlue);

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
