using DeskAssistant.ViewModels;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeskAssistant.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BirthdayTrackerPage : Page
    {
        public BirthdayTrackerViewModel ViewModel { get; }

        public BirthdayTrackerPage()
        {
            ViewModel = App.GetService<BirthdayTrackerViewModel>();

            this.InitializeComponent();

            DataContext = ViewModel;
        }
    }
}
