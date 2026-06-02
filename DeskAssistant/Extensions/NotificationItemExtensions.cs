using DeskAssistant.Core.Models;
using DeskAssistant.Models;
using DeskAssistant.ViewModels;
using GrpcService;
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
                NotificationTaskId = notificationEntity.NotificationTaskId,
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
                NotificationTaskId = notificationItem.NotificationTaskId,
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
                NotificationTaskId = viewModel.NotificationId,
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

        public NotificationEntity CalendarItemToNotificationEntity(string clientId, TaskItem task, Guid timerId)
        {
            var createdAt = DateTime.UtcNow;
            var dayOfWeek = createdAt.DayOfWeek;

            return new NotificationEntity
            {
                Id = task.Id,
                ClientId = clientId,
                NotificationTaskId = task.Id,
                IsEnabled = true,
                TimerId = timerId,
                NotificationTime = TimeSpan.Parse(task.ReminderTime),
                MondayEnabled = dayOfWeek == DayOfWeek.Monday,
                TuesdayEnabled = dayOfWeek == DayOfWeek.Tuesday,
                WednesdayEnabled = dayOfWeek == DayOfWeek.Wednesday,
                ThursdayEnabled = dayOfWeek == DayOfWeek.Thursday,
                FridayEnabled = dayOfWeek == DayOfWeek.Friday,
                SaturdayEnabled = dayOfWeek == DayOfWeek.Saturday,
                SundayEnabled = dayOfWeek == DayOfWeek.Sunday,
                CreatedAt = createdAt
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

        public NotificationItemStatus CalendarItemToNotificationItemStatus(CalendarTaskModel taskModel)
        {
            return new NotificationItemStatus
            {
                Id = taskModel.Id,
                ClientId = "DeskAssistant_Tasks",
                IsEnabled = taskModel.NotificationIsOn.ToString()
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
