using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeskAssistant.Core.Extensions;
using DeskAssistant.Core.Models;
using DeskAssistant.Helpers;
using DeskAssistant.Views;
using Grpc.Net.Client;
using GrpcService;
using Microsoft.UI.Xaml.Controls;
using NLog;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DeskAssistant.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private Frame _contentFrame;
        public event Action? TasksUpdated;
        private LoggerHelper _loggerHelper = new();
        private readonly TaskService.TaskServiceClient _grpcClient;
        private EnumExtensions _enumExtensions = new();

        // gRPC сервер в debug запускается на порту 5000
        private readonly GrpcChannel _grpcChannel = GrpcChannel.ForAddress("http://localhost:5000");
        // gRPC сервер в release запускается на порту 5218
        //private readonly GrpcChannel _grpcChannel = GrpcChannel.ForAddress("http://localhost:5218");

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
        public partial CalendarTaskModel CalendarTaskModel { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskModel> AllTasks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskModel> SelectedDayTasks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskModel> WeekTasks { get; set; }



        public CalendarViewModel()
        {
            _logger.Info($"gRPC client started with - [{_grpcChannel}] address");

            _grpcClient = new TaskService.TaskServiceClient(_grpcChannel);

            AllTasks = new();
            WeekTasks = new();
            SelectedDayTasks = new();

            AllTasks.CollectionChanged += OnTasksCollectionChanged;

            InitializeAsync();
        }

        private void InitializeAsync()
        {
            _loggerHelper.LogEnteringTheMethod();

            SelectedDate = DateOnly.FromDateTime(DateTime.Today);

            _ = GetAllTasksFromDbAsync();
            GetWeekPeriod();        
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
                    if (task != null)
                    {
                        task.CompletedDate = DateTime.UtcNow;
                        task.Status = TaskStatusEnum.Completed;

                        var request = CalendarTaskModelToGrpcTask(task);

                        await _grpcClient.UpdateTaskAsync(request);
                        await GetAllTasksFromDbAsync();
                    }
                }

                GetTasksForSelectedDate();
                OnPropertyChanged(nameof(WeekTasks));
                OnPropertyChanged(nameof(LableForTodayTasks));
                OnPropertyChanged(nameof(LableForWeekTasks));
            }
            catch (Exception ex)
            {
                _logger.Error($"Can`t update property - {ex}");
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
                        DueDate = newTask.DueDate.ToString("yyyy-MM-dd") ?? "",
                        Priority = _enumExtensions.PrioritiesLevelToString(newTask.Priority) ?? "",
                        Category = newTask.Category ?? "",
                        IsCompleted = newTask.IsCompleted.ToString() ?? "",
                        Status = _enumExtensions.StatusToString(newTask.Status) ?? "",
                        Tags = vm.DialogTags == null ? "#общие" : vm.DialogTags,
                        RecurrencePattern = "None"
                    };
                    
                    await _grpcClient.CreateTaskAsync(request);
                    await GetAllTasksFromDbAsync();
                }
                TasksUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error($"Can`t create task - {ex}");
            }            
        }        

        [RelayCommand]
        private void OpenMonthTasks()
        {
            _contentFrame.Content = new MonthTasksUserControl(this);
        }

        [RelayCommand]
        private void BackToCalendarView()
        {
            _contentFrame.Content = new CalendarUserControl(this);
        }

        private async Task GetAllTasksFromDbAsync()
        {
            _loggerHelper.LogEnteringTheMethod();

            try
            {
                if (AllTasks == null)
                    return;

                AllTasks.Clear();

                var emptyRequest = new EmptyRequest();

                var entities = await _grpcClient.GetAllTasksAsync(emptyRequest);
                foreach (var entity in entities.Tasks)
                {
                    var taskModel = new CalendarTaskModel
                    {
                        Id = string.IsNullOrEmpty(entity.Id) ? 0 : int.Parse(entity.Id),
                        Name = entity.Name,
                        Description = entity.Description,
                        DueDate = DateOnly.Parse(entity.DueDate),
                        IsCompleted = bool.Parse(entity.IsCompleted),
                        Priority = _enumExtensions.PrioritiesLevelFromString(entity.Priority),
                        Category = entity.Category,
                        Status = _enumExtensions.StatusFromString(entity.Status),
                        Tags = entity.Tags,
                        CreatedDate = entity.CreatedDate == "" ? null : DateTime.SpecifyKind(DateTime.Parse(entity.CreatedDate), DateTimeKind.Utc),
                        DueTime = entity.DueTime == "" ? null : TimeSpan.Parse(entity.DueTime),
                        ReminderTime = entity.ReminderTime == "" ? null : DateTime.SpecifyKind(DateTime.Parse(entity.ReminderTime), DateTimeKind.Utc),
                        CompletedDate = entity.CompletedDate == "" ? null : DateTime.SpecifyKind(DateTime.Parse(entity.CompletedDate), DateTimeKind.Utc),
                        IsRecurring = string.IsNullOrEmpty(entity.IsRecurring) ? false : bool.Parse(entity.IsRecurring),
                        RecurrencePattern = entity.RecurrencePattern,
                        Duration = entity.Duration == "" ? null : TimeSpan.Parse(entity.Duration)
                    };

                    AllTasks.Add(taskModel);
                }

                GetTasksForWeekPeriod();
                GetTasksForSelectedDate();
            }
            catch (Exception ex)
            {
                _logger.Error($"Can`t get all tasks from DB - [{ex}]");
            }            
        }

        private void GetWeekPeriod()
        {
            _loggerHelper.LogEnteringTheMethod();

            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)DateTime.Today.DayOfWeek + 7) % 7;
            WeekPeriod = DateOnly.FromDateTime(DateTime.Today).AddDays(daysUntilSunday);
        }

        public void GetTasksForSelectedDate()
        {
            _loggerHelper.LogEnteringTheMethod();

            if (AllTasks == null || SelectedDayTasks == null)
                return;

            SelectedDayTasks.Clear();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var selectedDate = SelectedDate;

            var taskForDate = AllTasks
                .Where(task => task.DueDate == selectedDate && !task.IsCompleted)
                .ToList();

            foreach (var task in taskForDate)
            {
                if (selectedDate == today)
                {
                    task.Status = TaskStatusEnum.InProgress;
                }

                //_ = _taskService.UpdateTaskAsync(task, task.Status);

                var request = CalendarTaskModelToGrpcTask(task);
                _ = _grpcClient.UpdateTaskAsync(request);
                SelectedDayTasks.Add(task);
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
                    task.Status = TaskStatusEnum.InProgress;
                }
                if (dueDate != today && !task.IsCompleted)
                {
                    task.Status = TaskStatusEnum.Pending;
                }
                if (dueDate >= today && dueDate <= WeekPeriod)
                {
                    WeekTasks.Add(task);
                }
            }
            WeekTasksCount = WeekTasks.Count;
        }

        public async Task OpenTaskDetails(CalendarTaskModel task)
        {
            var dialogVm = new AddTaskDialogViewModel
            {
                DialogName = task.Name,
                DialogDescription = task.Description,
                DialogDueDate = task.DueDate,
                DialogPriority = task.Priority,
                DialogCategory = task.Category,
                IsDoubleTapped = true,
                IsComboBoxDropdown = false
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
                DueDate = model.DueDate.ToString("yyyy-MM-dd"),
                IsCompleted = model.IsCompleted.ToString(),
                Priority = _enumExtensions.PrioritiesLevelToString(model.Priority),
                Category = model.Category,
                Status = _enumExtensions.StatusToString(model.Status),
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
    }
}
