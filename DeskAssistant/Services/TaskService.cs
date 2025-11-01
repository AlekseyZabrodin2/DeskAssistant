using DeskAssistant.DataBase;
using DeskAssistant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace DeskAssistant.Services
{
    public class TaskService
    {
        private readonly IDbContextFactory<TasksDbContext> _contextFactory;


        public TaskService(IDbContextFactory<TasksDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        public async Task<List<CalendarTaskEntity>> GetAllTasksAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            var response = await context.Tasks.ToListAsync();
            return response;
        }

        public async Task AddTaskForSelectedDate(CalendarTaskEntity taskEntity)
        {
            if (taskEntity.Name == string.Empty ||
                taskEntity.Description == null)
                return;

            using var context = _contextFactory.CreateDbContext();
            
            await context.Tasks.AddAsync(taskEntity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(CalendarTaskModel model, TaskStatus status)
        {
            using var context = _contextFactory.CreateDbContext();
            var entity = await context.Tasks.FindAsync(model.Id);
            if (entity != null)
            {
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.DueDate = model.DueDate;
                entity.IsCompleted = model.IsCompleted;
                entity.Priority = model.Priority;
                entity.Category = model.Category;
                entity.Status = status;
                entity.Tags = model.Tags;
                entity.DueTime = model.DueTime;
                entity.ReminderTime = model.ReminderTime;
                entity.CompletedDate = model.CompletedDate;
                entity.IsRecurring = model.IsRecurring;
                entity.RecurrencePattern = model.RecurrencePattern;
                entity.Duration = model.Duration;

                context.SaveChanges();
            }
        }

    }
}
