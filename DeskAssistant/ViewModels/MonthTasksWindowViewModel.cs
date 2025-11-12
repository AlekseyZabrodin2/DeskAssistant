using CommunityToolkit.Mvvm.ComponentModel;
using DeskAssistant.Core.Models;
using System.Collections.ObjectModel;

namespace DeskAssistant.ViewModels
{
    public partial class MonthTasksWindowViewModel : ObservableObject
    {

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskModel> MonthTasksAllTasks { get; set; }
        
    }
}
