using DeskAssistant.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace DeskAssistant.Services
{
    public class TaskService
    {
        private readonly TasksDbContext _context;


        public TaskService(TasksDbContext context)
        {
           _context = context;
        }


        public async Task<List<CalendarTaskEntity>> GetAllTasksAsync()
        {
            //_logger.Trace("Get All patients");

            var response = await _context.Tasks.ToListAsync();
            return response;
        }

        public async Task AddTaskForSelectedDateAsync()
        {
            var task = new CalendarTaskEntity 
            { 
                Name = "Обед",
                Description = "комплекс № 5",
                DueDate = DateTime.UtcNow.AddDays(9),
                Priority = "Normal",
                Category = "General",
                IsCompleted = true,
                Status = "Pending", 
                Tags = "tag1",
                RecurrencePattern = "None" 
            };
            
            _context.Tasks.Add(task);
            _context.SaveChanges();
        }



    }
}
