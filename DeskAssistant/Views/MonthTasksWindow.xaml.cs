using DeskAssistant.Models;
using DeskAssistant.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeskAssistant.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MonthTasksWindow : Window
{
    public MonthTasksWindowViewModel ViewModel { get; }

    public MonthTasksWindow(MonthTasksWindowViewModel viewModel)
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
                ViewModel.OpenTaskDetails(selectedTask);
            }
        }
    }
}
