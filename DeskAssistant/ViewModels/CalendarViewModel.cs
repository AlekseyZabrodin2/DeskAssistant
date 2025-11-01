using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeskAssistant.DataBase;
using DeskAssistant.Helpers;
using DeskAssistant.Models;
using DeskAssistant.Services;
using DeskAssistant.Views;
using Microsoft.UI.Xaml.Controls;
using NLog;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DeskAssistant.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly TaskService _taskService;
        public event Action? TasksUpdated;
        private LoggerHelper _loggerHelper = new();


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



        public CalendarViewModel(TaskService taskService)
        {
            _taskService = taskService;

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

            if (e.PropertyName == nameof(CalendarTaskModel.IsCompleted))
            {
                var task = sender as CalendarTaskModel;
                if (task != null)
                {
                    task.CompletedDate = DateTime.UtcNow;
                    await _taskService.UpdateTaskAsync(task, TaskStatus.Completed);
                    await GetAllTasksFromDbAsync();
                }
            }

            GetTasksForSelectedDate();
            OnPropertyChanged(nameof(WeekTasks));
            OnPropertyChanged(nameof(LableForTodayTasks));
            OnPropertyChanged(nameof(LableForWeekTasks));
        }

        [RelayCommand]
        private async Task CreateNewTask()
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
                    Priority = vm.DialogPriority ==  0 ? PrioritiesLevel.Низкий : vm.DialogPriority,
                    Category = vm.DialogCategory == null ? "Разное" : vm.DialogCategory,
                    IsCompleted = false,
                    Status = TaskStatus.Pending,
                    Tags = vm.DialogTags == null ? "#общие" : vm.DialogTags,
                    RecurrencePattern = "None"
                };

                vm.DialogName = string.Empty;
                vm.DialogDescription = string.Empty;
                vm.DialogPriority = 0;
                vm.DialogCategory = null;

                _taskService.AddTaskForSelectedDate(newTask);
                await GetAllTasksFromDbAsync();
            }
            TasksUpdated?.Invoke();
        }

        [RelayCommand]
        private void OpenMonthTasks()
        {
            var dialogVm = new MonthTasksWindowViewModel();
            dialogVm.MonthTasksAllTasks = AllTasks;

            var dialogControl = new MonthTasksWindow(dialogVm);
            dialogControl.Activate();
        }


        private async Task GetAllTasksFromDbAsync()
        {
            _loggerHelper.LogEnteringTheMethod();

            if (AllTasks == null)
                return;

            AllTasks.Clear();

            var entities = await _taskService.GetAllTasksAsync();
            foreach (var entity in entities)
            {
                var taskModel = new CalendarTaskModel
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    DueDate = entity.DueDate,
                    IsCompleted = entity.IsCompleted,
                    Priority = entity.Priority,
                    Category = entity.Category,
                    Status = entity.Status,
                    Tags = entity.Tags,
                    CreatedDate = entity.CreatedDate,
                    DueTime = entity.DueTime,
                    ReminderTime = entity.ReminderTime,
                    CompletedDate = entity.CompletedDate,
                    IsRecurring = entity.IsRecurring,
                    RecurrencePattern = entity.RecurrencePattern,
                    Duration = entity.Duration
                };

                AllTasks.Add(taskModel);
            }

            GetTasksForWeekPeriod();
            GetTasksForSelectedDate();
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
                    task.Status = TaskStatus.InProgress;
                }

                _ = _taskService.UpdateTaskAsync(task, task.Status);
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
                    task.Status = TaskStatus.InProgress;
                }
                if (dueDate != today && !task.IsCompleted)
                {
                    task.Status = TaskStatus.Pending;
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


    }
}
