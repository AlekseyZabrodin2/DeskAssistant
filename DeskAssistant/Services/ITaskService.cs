using DeskAssistant.DataBase;
using DeskAssistant.Models;

namespace DeskAssistant.Services
{
    public interface ITaskService
    {
        Task<CalendarTaskEntity> AddTaskAsync(CalendarTaskEntity task);
        Task<List<CalendarTaskEntity>> GetTasksForDateAsync(DateTime date);
        Task<CalendarTaskEntity> GetTaskByIdAsync(int id);
        Task<bool> DeleteTaskAsync(int id);

        Task<List<CalendarTaskEntity>> GetAllTasksAsync();
        Task AddTaskForSelectedDate(CalendarTaskEntity taskEntity);
        Task UpdateTaskAsync(CalendarTaskModel model, TaskStatus status);
    }
}
