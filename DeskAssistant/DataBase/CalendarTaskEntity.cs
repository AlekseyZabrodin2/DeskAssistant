namespace DeskAssistant.DataBase
{
    public class CalendarTaskEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow;
        public TimeSpan? DueTime { get; set; }
        public DateTime? ReminderTime { get; set; }
        public DateTime? CompletedDate { get; set; }

        public string Priority { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public string Status { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrencePattern { get; set; }

        public TimeSpan? Duration { get; set; }
    }
}
