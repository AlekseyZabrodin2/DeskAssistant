using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using NLog;

namespace DeskAssistant.Extensions
{
    public static class LoggerExtensions
    {
        public static void SetNotification(this object viewModel, string message, NotificationType type)
        {
            if (viewModel is not INotifiable notifiable)
                throw new InvalidOperationException($"ViewModel must implement {nameof(INotifiable)}");

            notifiable.NotificationMessageText = message;
            notifiable.NotificationMessageBrush = type switch
            {
                NotificationType.Success => new SolidColorBrush(Colors.Green),
                NotificationType.Warning => new SolidColorBrush(Colors.Orange),
                NotificationType.Error => new SolidColorBrush(Colors.Red),
                NotificationType.Info => new SolidColorBrush(Colors.Green),
                _ => new SolidColorBrush(Colors.Black)
            };
        }

        // Логирование + уведомление
        public static void LogAndNotify(this ILogger logger, object viewModel, string message, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Success:
                    logger.Info(message);
                    break;
                case NotificationType.Warning:
                    logger.Warn(message);
                    break;
                case NotificationType.Error:
                    logger.Error(message);
                    break;
                case NotificationType.Info:
                    logger.Info(message);
                    break;
            }

            viewModel.SetNotification(message, type);
        }
    }
}
