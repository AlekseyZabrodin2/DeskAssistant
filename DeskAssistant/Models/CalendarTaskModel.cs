using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskAssistant.Models
{
    public class CalendarTaskModel
    {
        public int Id { get; set; }

        // Основная информация
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Даты и время
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }           // Дата выполнения (ОБЯЗАТЕЛЬНО для календаря!)
        public TimeSpan? DueTime { get; set; }
        public DateTime? ReminderTime { get; set; }
        public DateTime? CompletedDate { get; set; }

        // Классификация
        public string Priority { get; set; } = "Medium"; // High, Medium, Low
        public string Category { get; set; } = "General"; // Work, Personal, Health, etc.
        public List<string> Tags { get; set; } = new();  // #работа, #здоровье
        public string Status { get; set; } = string.Empty;

        // Статус
        public bool IsCompleted { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrencePattern { get; set; } = string.Empty; // "Daily", "Weekly", "Monthly"

        // Для календарного отображения
        public TimeSpan? Duration { get; set; }
        public string Color { get; set; } = "#007ACC";


    }
}
