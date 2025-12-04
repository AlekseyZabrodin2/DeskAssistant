using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeskAssistant.Core.Extensions;
using DeskAssistant.Core.Models;
using DeskAssistant.Helpers;
using DeskAssistant.Views;
using Grpc.Net.Client;
using GrpcService;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NLog;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DeskAssistant.ViewModels
{
    public partial class CalendarViewModel : ObservableObject, IDisposable
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private Frame _contentFrame;
        public event Action? TasksUpdated;
        private LoggerHelper _loggerHelper = new();
        private TaskService.TaskServiceClient _grpcClient;
        private EnumExtensions _enumExtensions = new();
        private GrpcChannel _serverUrl; 
        private bool _disposed = false;

        // gRPC сервер в debug запускается на порту 5000
        private readonly GrpcChannel _grpcChannelDebug = GrpcChannel.ForAddress("http://localhost:5000");
        // gRPC сервер в release запускается на порту 5218
        private readonly GrpcChannel _grpcChannelRelease = GrpcChannel.ForAddress("http://localhost:5218");

        public string LableForTodayTasks
        {
            get => $"Задачи - [ {SelectedDate} ] ({DayTasksCount})";
        }

        public string LableForWeekTasks
        {
            get => $"Задачи недели [ {DateTime.Now.Date.ToShortDateString()} - {WeekPeriod} ] ({WeekTasksCount})";
        }

        [NotifyPropertyChangedFor(nameof(LableForTodayTasks))]
        [ObservableProperty]
        public partial int DayTasksCount { get; set; }

        [NotifyPropertyChangedFor(nameof(LableForWeekTasks))]
        [ObservableProperty]
        public partial int WeekTasksCount { get; set; }

        [NotifyPropertyChangedFor(nameof(LableForTodayTasks))]
        [ObservableProperty]
        public partial DateOnly SelectedDate { get; set; }

        [ObservableProperty]
        public partial DateOnly WeekPeriod { get; set; }

        [ObservableProperty]
        public partial string NotificationMessage { get; set; }

        [ObservableProperty]
        public partial string DiagnosticsMessage { get; set; }

        [ObservableProperty]
        public partial Brush NotificationMessageBrush { get; set; }

        [ObservableProperty]
        public partial Brush DiagnosticMessageBrush { get; set; }

        [ObservableProperty]
        public partial Brush EchoServerButtonBrush { get; set; }

        [ObservableProperty]
        public partial Brush EchoDataBaseButtonBrush { get; set; }

        [ObservableProperty]
        public partial CalendarTaskModel CalendarTaskModel { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskModel> AllTasks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskModel> MonthTasks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskModel> SelectedDayTasks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskModel> WeekTasks { get; set; }
                
        public AsyncRelayCommand? InitializeCommand { get; }


        public CalendarViewModel()
        {
            _grpcClient = GetAppEnvironment();

            AllTasks = new();
            MonthTasks = new();
            WeekTasks = new();
            SelectedDayTasks = new();

            AllTasks.CollectionChanged += OnTasksCollectionChanged;

            InitializeCommand = new AsyncRelayCommand(InitializeAsync);

            _ = InitializeCommand.ExecuteAsync(null);
        }


        public void InitializeFrame(Frame contentFrame)
        {
            _contentFrame = contentFrame;
        }

        

        private void OnTasksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _loggerHelper.LogEnteringTheMethod();

            if (e.NewItems != null)
            {
                foreach (CalendarTaskModel item in e.NewItems)
                {
                    item.PropertyChanged += OnTaskPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CalendarTaskModel item in e.OldItems)
                {
                    item.PropertyChanged -= OnTaskPropertyChanged;
                }
            }
        }

        private async void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _loggerHelper.LogEnteringTheMethod();

            try
            {
                if (e.PropertyName == nameof(CalendarTaskModel.IsCompleted))
                {
                    var task = sender as CalendarTaskModel;
                    if (task == null || !task.IsCompleted)
                        return;

                    task.CompletedDate = DateTime.UtcNow;
                    task.Status = _enumExtensions.StatusToString(TaskStatusEnum.Completed);

                    var request = CalendarTaskModelToGrpcTask(task);
                    await _grpcClient.UpdateTaskAsync(request);

                    await GetAllTasksFromDbAsync();
                    await GetTasksForSelectedDate();
                    OnPropertyChanged(nameof(WeekTasks));
                    OnPropertyChanged(nameof(LableForTodayTasks));
                    OnPropertyChanged(nameof(LableForWeekTasks));
                }
            }
            catch (Exception ex)
            {
                NotificationMessage = $"[ Can`t update property - {ex.InnerException?.Message} ]";
                NotificationMessageBrush = new SolidColorBrush(Colors.Red);
                _logger.Error(NotificationMessage);
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                _loggerHelper.LogEnteringTheMethod();
                GetWeekPeriodAndSelectedDate();
                await GetAllTasksFromDbAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка инициализации");
            }
        }

        [RelayCommand]
        private async Task CreateNewTask()
        {
            try
            {
                var dialogVm = new AddTaskDialogViewModel
                {
                    DialogDueDate = SelectedDate
                };

                var dialogControl = new AddTaskDialogControl(dialogVm);
                var dialog = new ContentDialog
                {
                    Title = "Добавить задачу",
                    PrimaryButtonText = "Сохранить",
                    CloseButtonText = "Отмена",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = dialogControl,
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var vm = dialogControl.ViewModel;
                    var newTask = new CalendarTaskEntity
                    {
                        Name = vm.DialogName,
                        Description = vm.DialogDescription,
                        CreatedDate = DateTime.UtcNow,
                        DueDate = vm.DialogDueDate,
                        Priority = vm.DialogPriority == 0 ? PrioritiesLevelEnum.Низкий : vm.DialogPriority,
                        Category = vm.DialogCategory == null ? "Разное" : vm.DialogCategory,
                        IsCompleted = false,
                        Status = TaskStatusEnum.Pending,
                        Tags = vm.DialogTags == null ? "#общие" : vm.DialogTags,
                        RecurrencePattern = "None"
                    };

                    var today = DateOnly.FromDateTime(DateTime.Today);
                    if (newTask.DueDate < today && !newTask.IsCompleted)
                    {
                        newTask.Status = TaskStatusEnum.Expired;
                    }

                    if (string.IsNullOrEmpty(vm.DialogName) ||
                        string.IsNullOrEmpty(vm.DialogDescription))
                        return;

                    vm.DialogName = string.Empty;
                    vm.DialogDescription = string.Empty;
                    vm.DialogPriority = 0;
                    vm.DialogCategory = null;

                    var request = new TaskItem
                    {
                        Name = newTask.Name ?? "",
                        Description = newTask.Description ?? "",
                        CreatedDate = newTask.CreatedDate.ToString() ?? "",
                        DueDate = newTask.DueDate.ToString("dd.MM.yyyy") ?? "",
                        Priority = _enumExtensions.PrioritiesLevelToString(newTask.Priority) ?? "",
                        Category = newTask.Category ?? "",
                        IsCompleted = newTask.IsCompleted.ToString() ?? "",
                        Status = _enumExtensions.StatusToString(newTask.Status) ?? "",
                        Tags = vm.DialogTags == null ? "#общие" : vm.DialogTags,
                        RecurrencePattern = "None"
                    };
                    
                    await _grpcClient.CreateTaskAsync(request);
                    await GetAllTasksFromDbAsync();

                    NotificationMessage = "[ Task create successfully ]";
                    NotificationMessageBrush = new SolidColorBrush(Colors.Green);
                }
                TasksUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                NotificationMessage = $"[ Can`t create task - {ex.InnerException?.Message} ]";
                NotificationMessageBrush = new SolidColorBrush(Colors.Red);
                _logger.Error(NotificationMessage);
            }            
        }        

        [RelayCommand]
        private async Task OpenMonthTasks()
        {
            await GetAllTasksFromDbAsync();

            MonthTasks.Clear();

            MonthTasks = new ObservableCollection<CalendarTaskModel>(
                AllTasks.Where(task => task.DueDate.Month == DateTime.Now.Month
                && task.DueDate.Year == DateTime.Now.Year));

            _logger.Info($"All tasks count - [{AllTasks.Count}], Month tasks count - [{MonthTasks.Count}] ");

            _contentFrame.Content = new MonthTasksUserControl(this);
        }

        [RelayCommand]
        private void BackToCalendarView()
        {
            _contentFrame.Content = new CalendarUserControl(this);
        }

        [RelayCommand]
        private async Task GetEchoGrpcServer()
        {
            await EchoServer();
        }

        [RelayCommand]
        private async Task GetEchoPgDataBase()
        {
            await EchoDataBase();
        }


        private TaskService.TaskServiceClient GetAppEnvironment()
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

            _ = EchoDataBase();
            _ = EchoServer();

            return _grpcClient;
        }
        
        private async Task GetAllTasksFromDbAsync()
        {
            _loggerHelper.LogEnteringTheMethod();

            try
            {
                bool getTasks = await GetTasksFromDb();
                if (!getTasks) 
                    return;

                GetTasksForWeekPeriod();
                await GetTasksForSelectedDate();

                NotificationMessage = "[ Get all tasks successfully ]";
                NotificationMessageBrush = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                NotificationMessage = $"[ Can`t get all tasks from DB - {ex.InnerException?.Message} ]";
                NotificationMessageBrush = new SolidColorBrush(Colors.Red);
                _logger.Error(NotificationMessage);
            }            
        }

        private async Task<bool> GetTasksFromDb()
        {
            if (AllTasks == null)
                return false;

            try
            {
                AllTasks.Clear();
                _logger.Info($"Коллекция всех задач очищена");

                var emptyRequest = new EmptyRequest();
                var entities = await _grpcClient.GetAllTasksAsync(emptyRequest);
                if (!entities.Success)
                {
                    NotificationMessage = $"[ {entities?.Message} ]";
                    NotificationMessageBrush = new SolidColorBrush(Colors.Red);
                    _logger.Error(NotificationMessage);
                    return false;
                }

                var existingIds = new HashSet<int>();

                foreach (var entity in entities.Tasks)
                {
                    var taskModel = TaskModelToCalendarTask(entity);

                    if (!existingIds.Contains(taskModel.Id))
                    {
                        if (AllTasks.Any(dayTask => dayTask.Id == taskModel.Id))
                        {
                            _logger.Warn($"❌ ДУБЛИКАТ: Попытка добавить дубликат задачи с номером [{taskModel.Id}] в AllTasks");
                            continue;
                        }

                        AllTasks.Add(taskModel);
                        existingIds.Add(taskModel.Id);

                        _logger.Info($"Задача с номером [{taskModel.Id}] добавлена");
                    }
                }

                await RefreshExpiredTasksAsync();

                return true;
            }
            catch (Exception ex)
            {
                NotificationMessage = $"[ Can`t get all tasks from DB - {ex.InnerException?.Message} ]";
                NotificationMessageBrush = new SolidColorBrush(Colors.Red);
                _logger.Error(NotificationMessage);

                return false;
            }            
        }

        private void GetWeekPeriodAndSelectedDate()
        {
            _loggerHelper.LogEnteringTheMethod();

            SelectedDate = DateOnly.FromDateTime(DateTime.Today);

            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)DateTime.Today.DayOfWeek + 7) % 7;
            WeekPeriod = DateOnly.FromDateTime(DateTime.Today).AddDays(daysUntilSunday);
        }

        public async Task GetTasksForSelectedDate()
        {
            _loggerHelper.LogEnteringTheMethod();

            if (AllTasks == null || SelectedDayTasks == null)
                return;

            SelectedDayTasks.Clear();
            _logger.Info($"Коллекция задач на сегодня очищена");

            var today = DateOnly.FromDateTime(DateTime.Today);
            var selectedDate = SelectedDate;

            var taskForDate = AllTasks
                .Where(task => task.DueDate == selectedDate && !task.IsCompleted)
                .ToList();

            foreach (var task in taskForDate)
            {
                if (selectedDate == today)
                {
                    task.Status = _enumExtensions.StatusToString(TaskStatusEnum.InProgress);
                    var request = CalendarTaskModelToGrpcTask(task);
                    await _grpcClient.UpdateTaskAsync(request);
                }

                //SelectedDayTasks.Add(task);
                //_logger.Info($"Задача с номером [{task.Id}] добавлена на сегодня");

                if (SelectedDayTasks.Any(dayTask => dayTask.Id == task.Id))
                {
                    _logger.Warn($"❌ ДУБЛИКАТ: Задача с номером [{task.Id}] уже добавлена в SelectedDayTasks");
                    continue;
                }

                SelectedDayTasks.Add(task);
                _logger.Info($"Задача с номером [{task.Id}] добавлена в SelectedDayTasks");
            }

            DayTasksCount = SelectedDayTasks.Count;

            TasksUpdated?.Invoke();
        }

        public void GetTasksForWeekPeriod()
        {
            _loggerHelper.LogEnteringTheMethod();

            if (AllTasks == null || WeekTasks == null)
                return;

            WeekTasks.Clear();

            var orderTasks = AllTasks
                .OrderByDescending(task => task.Priority)
                .ToList();

            foreach (var task in orderTasks)
            {
                var dueDate = task.DueDate;
                var today = DateOnly.FromDateTime(DateTime.Today);

                if (dueDate == today && !task.IsCompleted)
                {
                    task.Status = _enumExtensions.StatusToString(TaskStatusEnum.InProgress);
                }
                if (dueDate >= today && !task.IsCompleted)
                {
                    task.Status = _enumExtensions.StatusToString(TaskStatusEnum.Pending);
                }
                if (dueDate >= today && dueDate <= WeekPeriod)
                {
                    if (WeekTasks.Where(taskItem => taskItem.Id == task.Id).Any())
                    {
                        _logger.Info($"❌ ДУБЛИКАТ: Задача с номером [{task.Id}] уже добывлена в WeekTasks");
                        continue;
                    }

                    WeekTasks.Add(task);

                    _logger.Info($"Задача с номером [{task.Id}] успешно дабавлена в WeekTasks");
                }
            }
            WeekTasksCount = WeekTasks.Count;
        }

        private async Task RefreshExpiredTasksAsync()
        {
            try
            {
                var tasks = AllTasks.ToList();

                foreach (var task in tasks)
                {
                    var dueDate = task.DueDate;
                    var today = DateOnly.FromDateTime(DateTime.Today);

                    if (dueDate < today && !task.IsCompleted)
                    {
                        task.Status = _enumExtensions.StatusToString(TaskStatusEnum.Expired);

                        var request = CalendarTaskModelToGrpcTask(task);
                        await _grpcClient.UpdateTaskAsync(request);
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationMessage = $"[ Error checking expired tasks - {ex.InnerException?.Message} ]";
                NotificationMessageBrush = new SolidColorBrush(Colors.Red);
                _logger.Error(NotificationMessage);
            }            
        }

        public async Task OpenTaskDetails(CalendarTaskModel task)
        {
            var dialogVm = new AddTaskDialogViewModel
            {
                IsDoubleTapped = true,
                IsComboBoxDropdown = false,
                DialogName = task.Name,
                DialogDescription = task.Description,
                DialogDueDate = task.DueDate,
                DialogPriority = task.Priority,
                DialogCategory = task.Category                
            };

            var dialogControl = new AddTaskDialogControl(dialogVm);
            var dialog = new ContentDialog
            {
                Title = "Добавить задачу",
                CloseButtonText = "Закрыть",
                DefaultButton = ContentDialogButton.Primary,
                Content = dialogControl,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
        }
                
        private TaskItem CalendarTaskModelToGrpcTask(CalendarTaskModel model)
        {
            return new TaskItem
            {
                Id = model.Id.ToString(),
                Name = model.Name,
                Description = model.Description,
                DueDate = model.DueDate.ToString("dd.MM.yyyy"),
                IsCompleted = model.IsCompleted.ToString(),
                Priority = _enumExtensions.PrioritiesLevelToString(model.Priority),
                Category = model.Category,
                Status = model.Status,
                Tags = model.Tags,
                CreatedDate = model.CreatedDate.ToString(),
                DueTime = model.DueTime.ToString(),
                ReminderTime = model.ReminderTime.ToString(),
                CompletedDate = model.CompletedDate.ToString(),
                IsRecurring = model.IsRecurring.ToString(),
                RecurrencePattern = "None",
                Duration = model.Duration.ToString()
            };
        }

        private CalendarTaskModel TaskModelToCalendarTask(TaskItem entity)
        {
            if (entity == null)
            {
                _logger.Warn("TaskItem entity is null");
                return CreateDefaultCalendarTaskModel();
            }

            return new CalendarTaskModel
            {
                Id = string.IsNullOrEmpty(entity.Id) ? 0 : int.Parse(entity.Id),
                Name = entity.Name,
                Description = entity.Description,
                DueDate = string.IsNullOrEmpty(entity.DueDate) ? DateOnly.FromDateTime(DateTime.Now) : DateOnly.Parse(entity.DueDate),
                IsCompleted = entity.IsCompleted == null ? false : bool.Parse(entity.IsCompleted),
                Priority = _enumExtensions.PrioritiesLevelFromString(entity.Priority),
                Category = entity.Category,
                Status = entity.Status,
                Tags = entity.Tags,
                CreatedDate = entity.CreatedDate == "" ? null : DateTime.SpecifyKind(DateTime.Parse(entity.CreatedDate), DateTimeKind.Utc),
                DueTime = entity.DueTime == "" ? null : TimeSpan.Parse(entity.DueTime),
                ReminderTime = entity.ReminderTime == "" ? null : DateTime.SpecifyKind(DateTime.Parse(entity.ReminderTime), DateTimeKind.Utc),
                CompletedDate = entity.CompletedDate == "" ? null : DateTime.SpecifyKind(DateTime.Parse(entity.CompletedDate), DateTimeKind.Utc),
                IsRecurring = string.IsNullOrEmpty(entity.IsRecurring) ? false : bool.Parse(entity.IsRecurring),
                RecurrencePattern = entity.RecurrencePattern,
                Duration = entity.Duration == "" ? null : TimeSpan.Parse(entity.Duration)
            };
        }

        private CalendarTaskModel CreateDefaultCalendarTaskModel()
        {
            return new CalendarTaskModel
            {
                Id = 0,
                Name = "Default Task",
                Description = "Default description",
                DueDate = DateOnly.FromDateTime(DateTime.Today),
                IsCompleted = false,
                Priority = PrioritiesLevelEnum.Низкий,
                Category = "Default",
                Status = "Pending",
                Tags = "#default",
                CreatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
            };
        }

        private async Task<bool> EchoServer()
        {
            try
            {
                EchoServerButtonBrush = new SolidColorBrush(Colors.Gray);
                var emptyRequest = new EmptyRequest();
                var response = await _grpcClient.ServerEchoAsync(emptyRequest);
                DiagnosticsMessage = $"{_serverUrl.Target} - {response.Message}";
                EchoServerButtonBrush = new SolidColorBrush(Colors.Green);
                DiagnosticMessageBrush = new SolidColorBrush(Colors.Green);

                _ = InitializeAsync();

                return true;
            }
            catch (Exception ex)
            {
                EchoServerButtonBrush = new SolidColorBrush(Colors.Red);
                DiagnosticMessageBrush = new SolidColorBrush(Colors.Red);
                DiagnosticsMessage = "gRPC Server is not available";
                _logger.Error(ex, DiagnosticsMessage);

                return false;
            }
        }

        private async Task<bool> EchoDataBase()
        {
            try
            {
                EchoDataBaseButtonBrush = new SolidColorBrush(Colors.Gray);
                var emptyRequest = new EmptyRequest();
                var response = await _grpcClient.DataBaseEchoAsync(emptyRequest);
                if (!response.Success)
                {
                    SetErrorState(response.Message);
                    return false;
                }

                SetSuccessState(response.Message);

                _ = InitializeAsync();

                return true;
            }
            catch (Exception ex)
            {
                SetErrorState("Database connection failed", ex);
                return false;
            }
        }

        private void SetSuccessState(string message)
        {
            DiagnosticsMessage = $"{message}";
            EchoDataBaseButtonBrush = new SolidColorBrush(Colors.Green);
            DiagnosticMessageBrush = new SolidColorBrush(Colors.Green);
        }

        private void SetErrorState(string message, Exception ex = null)
        {
            EchoDataBaseButtonBrush = new SolidColorBrush(Colors.Red);
            DiagnosticMessageBrush = new SolidColorBrush(Colors.Red);
            DiagnosticsMessage = message;

            _logger.Error(DiagnosticsMessage);

            if (ex != null)
                _logger.Error(ex, message);
            else
                _logger.Error(message);
        }

        private void LogingAppEnvironment(string environment, string environmentType, GrpcChannel grpcChannel)
        {
            _logger.Info($"gRPC client trying to start in {environmentType} [{environment.ToLower()}] environment with - [{grpcChannel.Target}] address");
            _grpcClient = new TaskService.TaskServiceClient(grpcChannel);
            DiagnosticsMessage = $"gRPC trying start with - [{grpcChannel.Target}] address";
            DiagnosticMessageBrush = new SolidColorBrush(Colors.Gray);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                AllTasks.CollectionChanged -= OnTasksCollectionChanged;

                // Отписываемся от всех событий PropertyChanged
                foreach (var task in AllTasks)
                {
                    task.PropertyChanged -= OnTaskPropertyChanged;
                }

                _disposed = true;
            }
        }

        ~CalendarViewModel()
        {
            Dispose();
        }
    }
}
