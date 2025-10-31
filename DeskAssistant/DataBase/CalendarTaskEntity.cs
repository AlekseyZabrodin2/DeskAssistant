using CommunityToolkit.Mvvm.ComponentModel;

namespace DeskAssistant.DataBase
{
    public partial class CalendarTaskEntity : ObservableObject
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial string Name { get; set; }
        [ObservableProperty]
        public partial string Description { get; set; }

        [ObservableProperty]
        public partial DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [ObservableProperty]
        public partial DateOnly DueDate { get; set; }
        [ObservableProperty]
        public partial TimeSpan? DueTime { get; set; }
        [ObservableProperty]
        public partial DateTime? ReminderTime { get; set; }
        [ObservableProperty]
        public partial DateTime? CompletedDate { get; set; }

        [ObservableProperty]
        public partial PrioritiesLevel Priority { get; set; }
        [ObservableProperty]
        public partial string Category { get; set; }
        [ObservableProperty]
        public partial string Tags { get; set; }
        [ObservableProperty]
        public partial TaskStatus Status { get; set; }

        [ObservableProperty]
        public partial bool IsCompleted { get; set; }
        [ObservableProperty]
        public partial bool IsRecurring { get; set; }
        [ObservableProperty]
        public partial string RecurrencePattern { get; set; }

        [ObservableProperty]
        public partial TimeSpan? Duration { get; set; }
    }
}
