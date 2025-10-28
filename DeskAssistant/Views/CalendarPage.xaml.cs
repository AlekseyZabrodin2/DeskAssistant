using DeskAssistant.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

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

        public CalendarPage(CalendarViewModel viewModel)
        {
            ViewModel = viewModel;

            this.InitializeComponent();

            DataContext = ViewModel;
        }

        private void CalendarView_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (sender.SelectedDates.Any())
            {
                var selectedDate = sender.SelectedDates;
                ViewModel.SelectedDate = selectedDate.FirstOrDefault().Date;

                ViewModel.GetTasksForSelectedDate();
            }
        }

        private void CalendarView_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            var date = args.Item.Date.DateTime;

            // Проверяем, есть ли задачи на эту дату
            bool hasTasks = ViewModel.AllTasks.Any(t =>
                t.DueDate.Date == date.Date && !t.IsCompleted);

            // Находим элемент TaskDot в визуальном дереве
            if (args.Phase == 0 && args.Item is CalendarViewDayItem dayItem)
            {
                var taskDot = FindChild<Ellipse>(dayItem, "TaskDot");
                if (taskDot != null)
                {
                    taskDot.Visibility = hasTasks ? Visibility.Visible : Visibility.Collapsed;

                    // Меняем цвет точки по приоритету
                    if (hasTasks)
                    {
                        var highPriorityTasks = ViewModel.AllTasks.Any(t =>
                            t.DueDate.Date == date.Date && !t.IsCompleted );

                        //taskDot.Fill = highPriorityTasks ?
                        //    new SolidColorBrush(Colors.Red) :
                        //    new SolidColorBrush(Colors.Green);
                    }
                }
            }
        }

        // Вспомогательный метод для поиска в визуальном дереве
        private T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T foundElement && (child as FrameworkElement)?.Name == childName)
                    return foundElement;

                var result = FindChild<T>(child, childName);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
