using DeskAssistant.ViewModels;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeskAssistant.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalendarPage : Page
    {
        public CalendarViewModel ViewModel { get; set; }

        public CalendarPage()
        {
            ViewModel = App.GetService<CalendarViewModel>();

            this.InitializeComponent();

            DataContext = ViewModel;
        }
    }
}
