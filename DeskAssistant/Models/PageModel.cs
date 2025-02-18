using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskAssistant.Models
{
    public partial class PageModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Title { get; set; }

        [ObservableProperty]
        public partial string Tag { get; set; }

        [ObservableProperty]
        public partial string PathToDll { get; set; }
    }
}
