using CommunityToolkit.Mvvm.ComponentModel;
using DeskAssistant.Extensions;
using NLog;
using NotificationGrpcClient;
using System.Collections.ObjectModel;

namespace DeskAssistant.Models
{
    public partial class NotificationsCollectionModel : ObservableObject
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly NotificationItemExtensions _notificationExtensions = new();
        private NotificationService.NotificationServiceClient _grpcClient;
        private bool _notificationIsOnModel;


        [ObservableProperty]
        public partial bool IsInitializing { get; set; } = true;

        [ObservableProperty]
        public partial string NotificationIdModel { get; set; }

        [ObservableProperty]
        public partial string ClientIdModel { get; set; }

        public bool NotificationIsOnModel
        {
            get => _notificationIsOnModel;
            set
            {
                if (!IsInitializing)
                {
                    _notificationIsOnModel = value;
                    _ = SetNotificationStatus();
                }
                    
                SetProperty(ref _notificationIsOnModel, value);
            }
        }

        [ObservableProperty]
        public partial bool IsEnabledModel { get; set; }

        [ObservableProperty]
        public partial TimeSpan NotificationTimeModel { get; set; }
        [ObservableProperty]
        public partial bool MondayIsCheckedModel { get; set; }

        [ObservableProperty]
        public partial bool TuesdayIsCheckedModel { get; set; }

        [ObservableProperty]
        public partial bool WednesdayIsCheckedModel { get; set; }

        [ObservableProperty]
        public partial bool ThursdayIsCheckedModel { get; set; }

        [ObservableProperty]
        public partial bool FridayIsCheckedModel { get; set; }

        [ObservableProperty]
        public partial bool SaturdayIsCheckedModel { get; set; }

        [ObservableProperty]
        public partial bool SundayIsCheckedModel { get; set; }        

        [ObservableProperty]
        public partial ObservableCollection<NotificationsCollectionModel> NotificationCollectionModel { get; set; }



        public NotificationsCollectionModel(NotificationService.NotificationServiceClient grpcClient)
        {
            _grpcClient = grpcClient;
        }



        public async Task SetNotificationStatus()
        {
            try
            {
                var request = new NotificationItemStatus();

                request = _notificationExtensions.CollectionModelToNotificationItemStatus(this);
                var response = await _grpcClient.NotificationsSetStatusAsync(request);

                await GetNotificationsStatus();

                if(response.Success)
                    _logger.Error("Cтатуса уведомления успешно отправлен");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при отправке статуса уведомления в БД");
            }
        }

        public async Task GetNotificationsStatus()
        {
            try
            {
                var request = new NotificationClientIdRequest();
                request.ClientId = ClientIdModel;

                var notificationList = await _grpcClient.NotificationsGetSettingsAsync(request);
                if (!notificationList.Success)
                    return;

                foreach (var notification in notificationList.Notification)
                {
                    IsEnabledModel = bool.Parse(notification.IsEnabled);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при получении статуса уведомления");
            }
        }
    }
}
