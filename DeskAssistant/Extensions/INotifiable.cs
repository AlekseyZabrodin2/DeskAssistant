using Microsoft.UI.Xaml.Media;

namespace DeskAssistant.Extensions
{
    public interface INotifiable
    {
        string NotificationMessageText { get; set; }
        SolidColorBrush NotificationMessageBrush { get; set; }
    }
}
