using DeskAssistant.Core.Models;
using DeskAssistant.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeskAssistant.Views
{
    public sealed partial class MonthTasksUserControl : UserControl
    {
        public CalendarViewModel ViewModel { get; }

        public MonthTasksUserControl(CalendarViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }


        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender is WinUI.TableView.TableView tableView)
            {
                var originalSource = e.OriginalSource as FrameworkElement;
                var dataContext = originalSource?.DataContext;

                if (dataContext is CalendarTaskModel selectedTask)
                {
                    _ = ViewModel.OpenTaskDetailsAsync(selectedTask);
                }
            }
        }

    }
}
