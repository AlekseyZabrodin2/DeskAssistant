using CommunityToolkit.Mvvm.ComponentModel;
using DeskAssistant.DataBase;
using DeskAssistant.Services;
using System.Collections.ObjectModel;

namespace DeskAssistant.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        private readonly TasksDbContext _context;
        private readonly TaskService _taskService;
        private CalendarTaskEntity _entity = new();
        private bool _isCompleted;


        public string LableForTodayTasks
        {
            get => $"Задачи - [ {SelectedDate.Date.ToShortDateString()} ] ({DayTasksCount})";
        }

        public string LableForWeekTasks
        {
            get => $"Задачи недели [ {DateTime.Now.Date.ToShortDateString()} - {WeekPeriod.Date.ToShortDateString()} ] ({WeekTasksCount})";
        }

        [NotifyPropertyChangedFor(nameof(LableForTodayTasks))]
        [ObservableProperty]
        public partial int DayTasksCount { get; set; }

        [NotifyPropertyChangedFor(nameof(LableForWeekTasks))]
        [ObservableProperty]
        public partial int WeekTasksCount { get; set; }

        [NotifyPropertyChangedFor(nameof(LableForTodayTasks))]
        [ObservableProperty]
        public partial DateTime SelectedDate { get; set; } = DateTime.Today.Date;

        [ObservableProperty]
        public partial DateTime WeekPeriod { get; set; } = DateTime.Today;

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskEntity> AllTasks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskEntity> SelectedDayTasks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskEntity> WeekTasks { get; set; }



        public CalendarViewModel(TasksDbContext context)
        {
            _context = context;
            _taskService = new TaskService(context);

            InitializeAsync();
        }


        private void InitializeAsync()
        {
            AllTasks = new();
            WeekTasks = new();
            SelectedDayTasks = new();

            _ = GetAllTasksFromDbAsync();
            GetWeekPeriod();

            _ = _taskService.AddTaskForSelectedDateAsync();            
        }


        private async Task GetAllTasksFromDbAsync()
        {
            if (AllTasks == null)
                return;

            AllTasks.Clear();

            var allTasks = await _taskService.GetAllTasksAsync();
            foreach (var item in allTasks)
            {
                AllTasks.Add(item);
            }

            GetTasksForWeekPeriod();

            SelectedDate = DateTime.Now;
            GetTasksForSelectedDate();
        }

        private void GetWeekPeriod()
        {
            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)DateTime.Today.DayOfWeek + 7) % 7;
            WeekPeriod = DateTime.Today.AddDays(daysUntilSunday);
        }

        public void GetTasksForSelectedDate()
        {
            if (AllTasks == null || SelectedDayTasks == null) 
                return;

            SelectedDayTasks.Clear();

            foreach (var task in AllTasks)
            {
                var dueDate = task.DueDate.Date.ToShortDateString();
                var selectedDate = SelectedDate.Date.ToShortDateString();

                if (dueDate == selectedDate)
                {
                    SelectedDayTasks.Add(task);
                }
            }
            DayTasksCount = SelectedDayTasks.Count;
        }

        public void GetTasksForWeekPeriod()
        {
            if (AllTasks == null || WeekTasks == null)
                return;

            WeekTasks.Clear();

            foreach (var task in AllTasks)
            {
                var dueDate = task.DueDate.Date;

                if (dueDate >= DateTime.Today && dueDate <= WeekPeriod)
                {
                    WeekTasks.Add(task);
                }
            }
            WeekTasksCount = WeekTasks.Count;
        }






    }
}
