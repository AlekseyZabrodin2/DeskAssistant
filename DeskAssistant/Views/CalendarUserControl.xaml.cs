using DeskAssistant.Core.Models;
using DeskAssistant.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeskAssistant.Views
{
    public sealed partial class CalendarUserControl : UserControl
    {
        public CalendarViewModel ViewModel { get; }

        public CalendarUserControl(CalendarViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            DataContext = ViewModel;

            ViewModel.TasksUpdated += () =>
            {
                DispatcherQueue.TryEnqueue(UpdateAllCalendarDots);
            };
        }




        private void CalendarView_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (sender.SelectedDates.Any())
            {
                var selectedDate = sender.SelectedDates.First();
                ViewModel.SelectedDate = DateOnly.FromDateTime(selectedDate.DateTime);

                ViewModel.GetTasksForSelectedDateAsync();
            }
        }

        private void CalendarView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.TasksUpdated += () =>
            {
                DispatcherQueue.TryEnqueue(() => UpdateAllCalendarDots());
            };

            UpdateAllCalendarDots();
        }

        private void UpdateAllCalendarDots()
        {
            CalendarTaskUserControl.UpdateLayout();

            var dayItems = FindChildren<CalendarViewDayItem>(CalendarTaskUserControl);
            foreach (var dayItem in dayItems)
            {
                UpdateDotForDayItem(dayItem);
            }
        }

        // Вспомогательный метод для обновления одной даты
        private void UpdateDotForDayItem(CalendarViewDayItem dayItem)
        {
            var date = DateOnly.FromDateTime(dayItem.Date.DateTime);

            bool hasTasks = ViewModel.AllTasks.Any(t => t.DueDate == date && !t.IsCompleted);
            bool hasBirthdays = ViewModel.BirthdayPeoples.Any(t => DateOnly.FromDateTime(t.Birthday) == date);

            var taskDot = FindChild<Ellipse>(dayItem, "TaskDot");
            if (taskDot != null)
                taskDot.Visibility = hasTasks ? Visibility.Visible : Visibility.Collapsed;

            var birthdayDot = FindChild<Ellipse>(dayItem, "BirthdayDot");
            if (birthdayDot != null)
                birthdayDot.Visibility = hasBirthdays ? Visibility.Visible : Visibility.Collapsed;
        }

        public static IEnumerable<T> FindChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T t) yield return t;

                foreach (var descendant in FindChildren<T>(child))
                    yield return descendant;
            }
        }

        private void CalendarView_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            var date = DateOnly.FromDateTime(args.Item.Date.DateTime);

            // Проверяем, есть ли задачи на эту дату
            bool hasTasks = ViewModel.AllTasks.Any(t =>
                t.DueDate == date && !t.IsCompleted);

            bool hasBirthdays = ViewModel.BirthdayPeoples.Any(t => DateOnly.FromDateTime(t.Birthday) == date);

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
                            t.DueDate == date && !t.IsCompleted);
                    }
                }

                var birthdayDot = FindChild<Ellipse>(dayItem, "BirthdayDot");
                if (birthdayDot != null)
                {
                    birthdayDot.Visibility = hasBirthdays ? Visibility.Visible : Visibility.Collapsed;

                    // Меняем цвет точки по приоритету
                    if (hasBirthdays)
                    {
                        var highPriorityTasks = ViewModel.AllTasks.Any(t =>
                            t.DueDate == date && !t.IsCompleted);
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

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is CalendarTaskModel selectedTask)
            {
                _ = ViewModel.OpenTaskDetailsAsync(selectedTask);
            }
        }
    }
}
