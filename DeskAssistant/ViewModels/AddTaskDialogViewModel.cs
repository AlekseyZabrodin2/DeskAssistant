using CommunityToolkit.Mvvm.ComponentModel;

namespace DeskAssistant.ViewModels
{
    public partial class AddTaskDialogViewModel : ObservableObject
    {


        [ObservableProperty]
        public partial string DialogName { get; set; }

        [ObservableProperty]
        public partial string DialogDescription { get; set; }

        [ObservableProperty]
        public partial DateOnly DialogDueDate { get; set; }

        [ObservableProperty]
        public partial string DialogCategory { get; set; }

        [ObservableProperty]
        public partial string DialogTags { get; set; }

        [ObservableProperty]
        public partial PrioritiesLevel DialogPriority { get; set; }

        public Array PrioritiesOptions => Enum.GetValues(typeof(PrioritiesLevel));        

        public List<string> DialogCategories { get; } = new() { "Обеды", "Работа", "Дом", "Здоровье", "Учёба", "Развлечения", "Разное" };

        [ObservableProperty]
        public partial bool IsDoubleTapped { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanDropdownCanExecute))]
        [NotifyPropertyChangedFor(nameof(CanNotDropdownCanExecute))]
        public partial bool IsComboBoxDropdown { get; set; } = true;

        public bool CanDropdownCanExecute => !IsComboBoxDropdown;

        public bool CanNotDropdownCanExecute => IsComboBoxDropdown;





        partial void OnDialogCategoryChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            DialogTags = value switch
            {
                "Обеды" => "#oбеды",
                "Работа" => "#работа",
                "Дом" => "#дом",
                "Здоровье" => "#здоровье",
                "Учёба" => "#учёба",
                "Развлечения" => "#отдых",
                _ => "#общие"
            };

            OrderingDinerCategory(value);
        }

        private void OrderingDinerCategory(string value)
        {
            if (value == "Обеды" && !IsDoubleTapped)
            {
                DialogName = "Заказ обеда";
                DialogDescription = "Комплекс № - ";
                DialogPriority = PrioritiesLevel.Средний;
            }
        }
    }
}
