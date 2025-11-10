using CommunityToolkit.Mvvm.ComponentModel;
using DeskAssistant.Core.Models;
using DeskAssistant.Views;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace DeskAssistant.ViewModels
{
    public partial class MonthTasksWindowViewModel : ObservableObject
    {

        [ObservableProperty]
        public partial ObservableCollection<CalendarTaskModel> MonthTasksAllTasks { get; set; }

        

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
