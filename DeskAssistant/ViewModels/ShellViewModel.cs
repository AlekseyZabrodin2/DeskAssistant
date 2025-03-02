using CommunityToolkit.Mvvm.ComponentModel;
using DeskAssistant.Models;
using DeskAssistant.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;

namespace DeskAssistant.ViewModels
{
    public partial class ShellViewModel : ObservableRecipient
    {
        [ObservableProperty]
        public partial string SelectedPage { get; set; }

        [ObservableProperty]
        public partial string PageTitle { get; set; }

        [ObservableProperty]
        public partial string PageTag { get; set; }

        [ObservableProperty]
        public partial string PagePathToDll { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<PageModel> NamePagesCollection { get; set; }

        private readonly IServiceProvider _serviceProvider;


        public ShellViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            GetNavigationPages();
        }


        public void NavigationView(string pageTag, Frame frame)
        {

            SelectedPage = pageTag;

            if (pageTag == "Settings")
            {
                //frame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var currentPage = NamePagesCollection.FirstOrDefault(page => page.Tag == pageTag);
                //PagePathToDll = currentPage.PathToDll;

                frame.Navigate(typeof(BirthdayTrackerPage));
            }
        }

        public void GetNavigationPages()
        {

            NamePagesCollection = new ObservableCollection<PageModel>();

            var navigationSettings = _serviceProvider.GetRequiredService<IOptions<NavigationPages>>();
            var navigationPage = navigationSettings.Value;

            foreach (var page in navigationPage)
            {
                var pageModel = page.Value;

                NamePagesCollection.Add(new PageModel
                {
                    Title = pageModel.Title,
                    Tag = $"{pageModel.Title}Page",
                    PathToDll = pageModel.PathToDll,
                });

                PageTitle = pageModel.Title;
                PageTag = $"{pageModel.Title}Page";
                PagePathToDll = pageModel.PathToDll;
            }
        }
    }
}
