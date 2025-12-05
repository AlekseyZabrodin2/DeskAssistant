using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeskAssistant.Core.Models;
using DeskAssistant.Extensions;
using DeskAssistant.Models;
using Grpc.Net.Client;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using NLog;
using NotificationGrpcClient;
using System.Collections.ObjectModel;

namespace DeskAssistant.ViewModels
{
    public partial class SettingPageViewModel : ObservableObject
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private NotificationService.NotificationServiceClient _grpcClient;
        private readonly NotificationItemExtensions _notificationExtensions = new();

        private bool _notificationIsOn;
        private TimeSpan _notificationTime;
        private TimeSpan _selectedTime;
        private bool _mondayIsChecked = true;
        private bool _tuesdayIsChecked = true;
        private bool _wednesdayIsChecked = true;
        private bool _thursdayIsChecked = true;
        private bool _fridayIsChecked = true;
        private bool _saturdayIsChecked = false;
        private bool _sundayIsChecked = false;

        private GrpcChannel? _serverUrl;

        // gRPC сервер в debug запускается на порту 5000
        private readonly GrpcChannel _grpcChannelDebug = GrpcChannel.ForAddress("http://localhost:5000");
        // gRPC сервер в release запускается на порту 5218
        private readonly GrpcChannel _grpcChannelRelease = GrpcChannel.ForAddress("http://localhost:5218");

        public string ClientId { get; } = "DeskAssistant";

        [ObservableProperty]
        public partial string NotificationId { get; set; }

        public bool NotificationIsOn
        {
            get => _notificationIsOn;
            set
            {
                SettingsChanged = true;
                SetProperty(ref _notificationIsOn, value);
            }
        }
        
        public TimeSpan NotificationTime
        {
            get => _notificationTime;
            set
            {
                SettingsChanged = true;
                SetProperty(ref _notificationTime, value);
                SelectedTime = _notificationTime;
            }
        }

        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                SettingsChanged = true;
                SetProperty(ref _selectedTime, value);
            }
        }

        public bool MondayIsChecked
        {
            get => _mondayIsChecked;
            set
            {
                SettingsChanged = true;
                SetProperty(ref _mondayIsChecked, value);                
            }
        }
        
        public bool TuesdayIsChecked
        {
            get => _tuesdayIsChecked;
            set
            {
                SettingsChanged = true;
                SetProperty(ref _tuesdayIsChecked, value);
            }
        }
        
        public bool WednesdayIsChecked
        {
            get => _wednesdayIsChecked;
            set
            {
                SettingsChanged = true;
                SetProperty(ref _wednesdayIsChecked, value);
            }
        }

        public bool ThursdayIsChecked
        {
            get => _thursdayIsChecked;
            set
            {
                SettingsChanged = true;
                SetProperty(ref _thursdayIsChecked, value);
            }
        }

        public bool FridayIsChecked
        {
            get => _fridayIsChecked;
            set
            {
                SettingsChanged = true;
                SetProperty(ref _fridayIsChecked, value);
            }
        }

        public bool SaturdayIsChecked
        {
            get => _saturdayIsChecked;
            set
            {
                SettingsChanged = true;
                SetProperty(ref _saturdayIsChecked, value);
            }
        }

        public bool SundayIsChecked
        {
            get => _sundayIsChecked;
            set
            {
                SettingsChanged = true;
                SetProperty(ref _sundayIsChecked, value);
            }
        }

        [ObservableProperty]
        public partial bool SaveButtonIsEnabled { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveNotificationSettingsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelSettingsChangeCommand))]
        public partial bool SettingsChanged { get; set; }

        [ObservableProperty]
        public partial bool NotificationIsStarted { get; set; }

        [ObservableProperty]
        public partial string NotificationMessageText { get; set; }

        [ObservableProperty]
        public partial Brush NotificationMessageBrush { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<NotificationEntity> NotificationCollection { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<NotificationsCollectionModel> NotificationCollectionModel { get; set; }

        [ObservableProperty]
        public partial NotificationsCollectionModel NotificationsCollectionModel {  get; set; }



        public SettingPageViewModel()
        {
            NotificationCollection = new();
            NotificationCollectionModel = new();

            _grpcClient = GetAppEnvironment();

            _ = GetSettingsFromServer();

            SettingsChanged = false;

            NotificationsCollectionModel = new(_grpcClient);
        }
        


        [RelayCommand(CanExecute = nameof(SettingsChanged))]
        public async Task SaveNotificationSettings()
        {
            SettingsChanged = false;

            await SaveNotificationsInDb();
        }

        [RelayCommand(CanExecute = nameof(SettingsChanged))]
        public async Task CancelSettingsChange()
        {
            ResetSettingsToDefolt();
        }

        [RelayCommand()]
        public async Task DeleteNotification(NotificationsCollectionModel model)
        {
            try
            {
                var request = new NotificationItemId();

                request.ClientId = model.ClientIdModel;
                request.Id = model.NotificationIdModel;

                var response = await _grpcClient.NotificationsDeleteAsync(request);

                if (response.Success)
                {
                    var item = _notificationExtensions.CollectionModelToNotificationEntity(model);

                    NotificationCollectionModel.Remove(model);
                    _logger.Error("Уведомление успешно удалено");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при удалении уведомления из БД");
            }
        }

        private async Task SaveNotificationsInDb()
        {
            if (!NotificationIsOn)
            {
                _logger.Info("Уведомления отключены, пропускаем сохранение");
                SetWarningMessage("Уведомление ВЫКЛЮЧЕНО, пропускаем сохранение");

                return;
            }
            try
            {
                NotificationId = $"{NotificationCollection.Count}-{ClientId}_{DateTime.Now:ddMMyyyyHHmmssfff}";

                var settings = _notificationExtensions.SettingsToNotificationEntity(this);
                var notificationItem = _notificationExtensions.NotificationEntityToNotificationItem(settings);

                var response = await _grpcClient.NotificationsCreateAsync(notificationItem);

                if (response.Success)
                {
                    _logger.Info($"✅ Уведомление {settings.Id} успешно сохранено");

                    SetSuccessMessage("Уведомление успешно сохранено");

                    // Обновляем локальную коллекцию только при успешном сохранении
                    var notificationEntity = _notificationExtensions.GrpcNotificationItemToNotificationEntity(notificationItem);
                    NotificationCollection.Add(notificationEntity);

                    var notificationModel = NotificationEntityToNotificationCollectionModel(notificationEntity);
                    NotificationCollectionModel.Add(notificationModel);
                    notificationModel.IsInitializing = false;
                }

                ResetSettingsToDefolt();
            }
            catch (Exception ex)
            {
                var message = $"{(string.IsNullOrEmpty(ex.InnerException.Message) ? "Ошибка при сохранении уведомления в БД" : ex.InnerException.Message)}";

                SetErrorMessage(message);
                _logger.Error(message);
            }
        }

        public async Task GetSettingsFromServer()
        {
            var request = new NotificationClientIdRequest();
            request.ClientId = ClientId;

            NotificationCollection.Clear();
            NotificationCollectionModel.Clear();

            var notificationList = await _grpcClient.NotificationsGetSettingsAsync(request);
            if (!notificationList.Success)
                return;

            var sortedNotifications = notificationList.Notification.OrderByDescending(item => item.IsEnabled);

            foreach (var notification in sortedNotifications)
            {
                var notificationEntity = _notificationExtensions.GrpcNotificationItemToNotificationEntity(notification);
                NotificationCollection.Add(notificationEntity);

                var notificationModel = NotificationEntityToNotificationCollectionModel(notificationEntity);
                NotificationCollectionModel.Add(notificationModel);
                notificationModel.IsInitializing = false;
            }
        }
        
        public void ResetSettingsToDefolt()
        {
            NotificationIsOn = false;
            NotificationTime = TimeSpan.Zero;
            MondayIsChecked = true;
            TuesdayIsChecked = true;
            WednesdayIsChecked = true;
            ThursdayIsChecked = true;
            FridayIsChecked = true;
            SaturdayIsChecked = false;
            SundayIsChecked = false;

            SettingsChanged = false;
        }

        public NotificationsCollectionModel NotificationEntityToNotificationCollectionModel(NotificationEntity notificationEntity)
        {
            return new NotificationsCollectionModel(_grpcClient)
            {
                IsInitializing = true,
                ClientIdModel = notificationEntity.ClientId,
                NotificationIdModel = notificationEntity.Id,
                NotificationIsOnModel = notificationEntity.IsEnabled,
                IsEnabledModel = notificationEntity.IsEnabled,
                NotificationTimeModel = notificationEntity.NotificationTime,
                MondayIsCheckedModel = notificationEntity.MondayEnabled,
                TuesdayIsCheckedModel = notificationEntity.TuesdayEnabled,
                WednesdayIsCheckedModel = notificationEntity.WednesdayEnabled,
                ThursdayIsCheckedModel = notificationEntity.ThursdayEnabled,
                FridayIsCheckedModel = notificationEntity.FridayEnabled,
                SaturdayIsCheckedModel = notificationEntity.SaturdayEnabled,
                SundayIsCheckedModel = notificationEntity.SundayEnabled
            };
        }


        private NotificationService.NotificationServiceClient GetAppEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                   ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                   ?? "Production";

            _logger.Info($"Application environment: {environment}");

            switch (environment.ToLower())
            {
                case "development":
                    _serverUrl = _grpcChannelDebug;
                    LogingAppEnvironment(environment, string.Empty, _serverUrl);
                    break;
                case "production":
                    _serverUrl = _grpcChannelRelease;
                    LogingAppEnvironment(environment, string.Empty, _serverUrl);
                    break;
                default:
                    _serverUrl = _grpcChannelRelease;
                    LogingAppEnvironment(environment, "DEFAULT", _serverUrl);
                    break;
            }

            return _grpcClient;
        }

        private void LogingAppEnvironment(string environment, string environmentType, GrpcChannel grpcChannel)
        {
            _logger.Info($"gRPC client trying to start in {environmentType} [{environment.ToLower()}] environment with - [{grpcChannel.Target}] address");
            _grpcClient = new NotificationService.NotificationServiceClient(grpcChannel);
        }

        private void SetSuccessMessage(string message)
        {
            NotificationMessageText = $"{message}";
            NotificationMessageBrush = new SolidColorBrush(Colors.Green);
        }

        private void SetWarningMessage(string message)
        {
            NotificationMessageText = $"{message}";
            NotificationMessageBrush = new SolidColorBrush(Colors.Orange);
        }

        private void SetErrorMessage(string message)
        {
            NotificationMessageText = $"{message}";
            NotificationMessageBrush = new SolidColorBrush(Colors.Red);
        }
    }
}
