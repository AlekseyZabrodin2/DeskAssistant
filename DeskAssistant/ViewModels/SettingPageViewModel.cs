using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeskAssistant.Core.Models;
using DeskAssistant.Extensions;
using Grpc.Net.Client;
using Microsoft.UI.Xaml;
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


        public bool NotificationIsOn
        {
            get => _notificationIsOn;
            set
            {
                SettingsSaved = true;
                SetProperty(ref _notificationIsOn, value);
            }
        }
        
        public TimeSpan NotificationTime
        {
            get => _notificationTime;
            set
            {
                SettingsSaved = true;
                SetProperty(ref _notificationTime, value);
                SelectedTime = _notificationTime;
            }
        }

        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                SettingsSaved = true;
                SetProperty(ref _selectedTime, value);
            }
        }

        public bool MondayIsChecked
        {
            get => _mondayIsChecked;
            set
            {
                SettingsSaved = true;
                SetProperty(ref _mondayIsChecked, value);                
            }
        }
        
        public bool TuesdayIsChecked
        {
            get => _tuesdayIsChecked;
            set
            {
                SettingsSaved = true;
                SetProperty(ref _tuesdayIsChecked, value);
            }
        }
        
        public bool WednesdayIsChecked
        {
            get => _wednesdayIsChecked;
            set
            {
                SettingsSaved = true;
                SetProperty(ref _wednesdayIsChecked, value);
            }
        }

        public bool ThursdayIsChecked
        {
            get => _thursdayIsChecked;
            set
            {
                SettingsSaved = true;
                SetProperty(ref _thursdayIsChecked, value);
            }
        }

        public bool FridayIsChecked
        {
            get => _fridayIsChecked;
            set
            {
                SettingsSaved = true;
                SetProperty(ref _fridayIsChecked, value);
            }
        }

        public bool SaturdayIsChecked
        {
            get => _saturdayIsChecked;
            set
            {
                SettingsSaved = true;
                SetProperty(ref _saturdayIsChecked, value);
            }
        }

        public bool SundayIsChecked
        {
            get => _sundayIsChecked;
            set
            {
                SettingsSaved = true;
                SetProperty(ref _sundayIsChecked, value);
            }
        }

        [ObservableProperty]
        public partial bool SaveButtonIsEnabled { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveNotificationSettingsCommand))]
        public partial bool SettingsSaved { get; set; }

        [ObservableProperty]
        public partial bool NotificationIsStarted { get; set; }

        [ObservableProperty]
        private partial DispatcherTimer AlarmTimer {  get; set; }

        [ObservableProperty]
        public partial ObservableCollection<NotificationEntity> NotificationCollection { get; set; }



        public SettingPageViewModel()
        {
            NotificationCollection = new();

            _grpcClient = GetAppEnvironment();

            _ = GetSettingsFromServer();

            SettingsSaved = false;
        }
        


        [RelayCommand(CanExecute = nameof(SettingsSaved))]
        public async Task SaveNotificationSettings()
        {
            SettingsSaved = false;

            await SaveNotificationsInDb();
        }


        private async Task SaveNotificationsInDb()
        {
            if (!NotificationIsOn)
            {
                _logger.Info("Уведомления отключены, пропускаем сохранение");
                return;
            }
            try
            {
                var settings = _notificationExtensions.SettingsToNotificationEntity(this);
                settings.Id = $"{NotificationCollection.Count}-{ClientId}_{DateTime.Now:ddMMyyyyHHmmssfff}";
                var notificationItem = _notificationExtensions.NotificationEntityToNotificationItem(settings);

                var response = await _grpcClient.NotificationsCreateAsync(notificationItem);

                if (response.Success)
                {
                    _logger.Info($"✅ Уведомление {settings.Id} успешно сохранено");

                    // Обновляем локальную коллекцию только при успешном сохранении
                    var notificationEntity = _notificationExtensions.GrpcNotificationItemToNotificationEntity(notificationItem);

                    NotificationCollection.Add(notificationEntity);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при сохранении уведомления в БД");
            }
        }

        public async Task GetSettingsFromServer()
        {
            var request = new NotificationClientIdRequest();
            request.ClientId = ClientId;

            NotificationCollection.Clear();

            var notificationList = await _grpcClient.NotificationsGetSettingsAsync(request);
            if (!notificationList.Success)
                return;

            foreach (var notification in notificationList.Notification)
            {
                var notificationEntity = _notificationExtensions.GrpcNotificationItemToNotificationEntity(notification);
                NotificationCollection.Add(notificationEntity);
            }
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
    }
}
