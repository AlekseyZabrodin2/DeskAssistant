using CommunityToolkit.Mvvm.ComponentModel;

namespace DeskAssistant.Models
{
    public partial class CalendarTaskModel : ObservableObject
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string Description { get; set; } = string.Empty;

        [ObservableProperty]
        public partial DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        [ObservableProperty]
        public partial DateOnly DueDate { get; set; }

        public string DueDateFormatted => DueDate.ToString("dd.MM");

        [ObservableProperty]
        public partial TimeSpan? DueTime { get; set; }
        [ObservableProperty]
        public partial DateTime? ReminderTime { get; set; }
        [ObservableProperty]
        public partial DateTime? CompletedDate { get; set; }

        [ObservableProperty]
        public partial PrioritiesLevel Priority { get; set; }
        [ObservableProperty]
        public partial string Category { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string Tags { get; set; } = string.Empty;
        [ObservableProperty]
        public partial TaskStatus Status { get; set; }

        [ObservableProperty]
        public partial bool IsCompleted { get; set; }
        [ObservableProperty]
        public partial bool IsRecurring { get; set; }
        [ObservableProperty]
        public partial string RecurrencePattern { get; set; } = string.Empty;

        [ObservableProperty]
        public partial TimeSpan? Duration { get; set; }

    }
}
