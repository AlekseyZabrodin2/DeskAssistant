using CommunityToolkit.Mvvm.ComponentModel;

namespace DeskAssistant.Models
{
    public partial class DayOfWeekItem : ObservableObject
    {
        [ObservableProperty]
        public partial string Name { get; set; }

        [ObservableProperty]
        public partial bool IsChecked { get; set; }
    }
}
