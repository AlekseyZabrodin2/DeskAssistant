using DeskAssistant.Core.Models;
using DeskAssistant.Models;
using DeskAssistant.ViewModels;
using NotificationGrpcClient;

namespace DeskAssistant.Extensions
{
    public class NotificationItemExtensions
    {
        public NotificationItem NotificationEntityToNotificationItem(NotificationEntity notificationEntity)
        {
            return new NotificationItem
            {
                Id = notificationEntity.Id.ToString(),
                ClientId = notificationEntity.ClientId.ToString(),
                IsEnabled = notificationEntity.IsEnabled.ToString(),
                TimerId = notificationEntity.TimerId.ToString(),
                NotificationTime = notificationEntity.NotificationTime.ToString(),
                MondayEnabled = notificationEntity.MondayEnabled.ToString(),
                TuesdayEnabled = notificationEntity.TuesdayEnabled.ToString(),
                WednesdayEnabled = notificationEntity.WednesdayEnabled.ToString(),
                ThursdayEnabled = notificationEntity.ThursdayEnabled.ToString(),
                FridayEnabled = notificationEntity.FridayEnabled.ToString(),
                SaturdayEnabled = notificationEntity.SaturdayEnabled.ToString(),
                SundayEnabled = notificationEntity.SundayEnabled.ToString(),
                CreatedAt = notificationEntity.CreatedAt.ToString("O")
            };
        }

        public NotificationEntity GrpcNotificationItemToNotificationEntity(NotificationItem notificationItem)
        {
            return new NotificationEntity
            {
                Id = notificationItem.Id,
                ClientId = notificationItem.ClientId,
                IsEnabled = bool.Parse(notificationItem.IsEnabled),
                TimerId = Guid.Parse(notificationItem.TimerId),
                NotificationTime = TimeSpan.Parse(notificationItem.NotificationTime),
                MondayEnabled = bool.Parse(notificationItem.MondayEnabled),
                TuesdayEnabled = bool.Parse(notificationItem.TuesdayEnabled),
                WednesdayEnabled = bool.Parse(notificationItem.WednesdayEnabled),
                ThursdayEnabled = bool.Parse(notificationItem.ThursdayEnabled),
                FridayEnabled = bool.Parse(notificationItem.FridayEnabled),
                SaturdayEnabled = bool.Parse(notificationItem.SaturdayEnabled),
                SundayEnabled = bool.Parse(notificationItem.SundayEnabled),
                CreatedAt = DateTime.Parse(notificationItem.CreatedAt)
            };
        }

        public NotificationEntity SettingsToNotificationEntity(SettingPageViewModel viewModel)
        {
            return new NotificationEntity
            {
                Id = viewModel.NotificationId,
                ClientId = viewModel.ClientId,
                IsEnabled = viewModel.NotificationIsOn,
                TimerId = viewModel.TimerId,
                NotificationTime = viewModel.SelectedTime,
                MondayEnabled = viewModel.MondayIsChecked,
                TuesdayEnabled = viewModel.TuesdayIsChecked,
                WednesdayEnabled = viewModel.WednesdayIsChecked,
                ThursdayEnabled = viewModel.ThursdayIsChecked,
                FridayEnabled = viewModel.FridayIsChecked,
                SaturdayEnabled = viewModel.SaturdayIsChecked,
                SundayEnabled = viewModel.SundayIsChecked,
                CreatedAt = DateTime.UtcNow
            };
        }

        public NotificationItemStatus CollectionModelToNotificationItemStatus(NotificationsCollectionModel collectionModel)
        {
            return new NotificationItemStatus
            {
                Id = collectionModel.NotificationIdModel,
                ClientId = collectionModel.ClientIdModel,
                IsEnabled = collectionModel.NotificationIsOnModel.ToString()
            };
        }

        public NotificationEntity CollectionModelToNotificationEntity(NotificationsCollectionModel collectionModel)
        {
            return new NotificationEntity
            {
                Id = collectionModel.NotificationIdModel,
                ClientId = collectionModel.ClientIdModel,
                IsEnabled = collectionModel.NotificationIsOnModel
            };
        }
    }
}
