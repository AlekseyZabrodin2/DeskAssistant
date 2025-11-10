using DeskAssistant.ViewModels;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeskAssistant.Views;

public sealed partial class AddTaskDialogControl : UserControl
{
    public AddTaskDialogViewModel ViewModel { get; }

    public AddTaskDialogControl(AddTaskDialogViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
        this.DataContext = viewModel;
    }
}
