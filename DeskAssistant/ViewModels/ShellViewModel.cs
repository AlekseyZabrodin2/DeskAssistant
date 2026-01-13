using BirthdaysGrpcService;
using CommunityToolkit.Mvvm.ComponentModel;
using DeskAssistant.Models;
using DeskAssistant.Views;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog;
using System.Collections.ObjectModel;
using Frame = Microsoft.UI.Xaml.Controls.Frame;

namespace DeskAssistant.ViewModels
{
    public partial class ShellViewModel : ObservableRecipient
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private BirthdayService.BirthdayServiceClient _grpcClient;
        private GrpcChannel _serverUrl;

        // gRPC сервер в debug запускается на порту 5000
        private readonly GrpcChannel _grpcChannelDebug = GrpcChannel.ForAddress("http://localhost:5000");
        // gRPC сервер в release запускается на порту 5218
        private readonly GrpcChannel _grpcChannelRelease = GrpcChannel.ForAddress("http://localhost:5218");

        [ObservableProperty]
        public partial string SelectedPage { get; set; }

        [ObservableProperty]
        public partial string PageTitle { get; set; }

        [ObservableProperty]
        public partial string PageIcon { get; set; }

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
            _grpcClient = GetAppEnvironment();

            string filePath = "Data\\SpisokORPK.docx";
            _ = ReadTextFromDocx(Path.Combine(AppContext.BaseDirectory, filePath));

            GetNavigationPages();
        }


        public void NavigationView(string pageTag, Frame frame, IServiceProvider serviceProvider)
        {

            SelectedPage = pageTag;

            if (pageTag == "Settings")
            {
                frame.Navigate(typeof(SettingsPage));
            }
            else if (pageTag == "Calendar PlannerPage")
            {
                using var scope = serviceProvider.CreateScope();
                var calendarPage = scope.ServiceProvider.GetService<CalendarPage>();
                frame.Content = calendarPage;
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
                    IconText = pageModel.IconText,
                    Tag = $"{pageModel.Title}Page",
                    PathToDll = pageModel.PathToDll,
                });

                PageTitle = pageModel.Title;
                PageIcon = pageModel.IconText;
                PageTag = $"{pageModel.Title}Page";
                PagePathToDll = pageModel.PathToDll;
            }
        }

        public async Task ReadTextFromDocx(string filePath)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                var textList = wordDoc.MainDocumentPart.Document.Body
                    .Descendants<Paragraph>()
                    .Where(p => !string.IsNullOrWhiteSpace(p.InnerText))
                    .Select(p => p.InnerText.Trim())
                    .ToList();

                await GetBirthdayPeoples(textList);
            }
        }

        public async Task GetBirthdayPeoples(List<string> textList)
        {
            try
            {
                var birthdayPeoplesCount = 1;

                for (int i = 0; i < textList.Count; i += 3)
                {
                    var fullName = textList[i].Trim().Split(' ');

                    var lastname = fullName[0];
                    var name = fullName[1];
                    var middleName = fullName.Length > 2 ? fullName[2] : "";

                    var birthday = textList[i + 1];

                    var email = textList[i + 2];

                    var birthdayPeople = new BirthdayPeopleModel
                    {
                        Id = birthdayPeoplesCount++,
                        LastName = lastname,
                        Name = name,
                        MiddleName = middleName,
                        Birthday = DateTime.Parse(birthday),
                        Email = email
                    };

                    var request = new BirthdayItem
                    {
                        Id = birthdayPeople.Id.ToString(),
                        LastName = birthdayPeople.LastName,
                        Name = birthdayPeople.Name,
                        MiddleName = birthdayPeople.MiddleName,
                        Birthday = birthdayPeople.Birthday.ToShortDateString(),
                        Email = birthdayPeople.Email
                    };

                    await _grpcClient.SetBirthdaysAsync(request);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Can`t send Birthday to DB - [{ex.InnerException.Message}]");
            }            
        }

        private BirthdayService.BirthdayServiceClient GetAppEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                   ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                   ?? "Production";

            _logger.Info($"Application environment: {environment}");

            switch (environment.ToLower())
            {
                case "development":
                    _serverUrl = _grpcChannelDebug;
                    LogingAppEnvironment(environment, string.Empty, _serverUrl);
                    break;
                case "production":
                    _serverUrl = _grpcChannelRelease;
                    LogingAppEnvironment(environment, string.Empty, _serverUrl);
                    break;
                default:
                    _serverUrl = _grpcChannelRelease;
                    LogingAppEnvironment(environment, "DEFAULT", _serverUrl);
                    break;
            }

            return _grpcClient;
        }

        private void LogingAppEnvironment(string environment, string environmentType, GrpcChannel grpcChannel)
        {
            _logger.Info($"gRPC client trying to start in {environmentType} [{environment.ToLower()}] environment with - [{grpcChannel.Target}] address");
            _grpcClient = new BirthdayService.BirthdayServiceClient(grpcChannel);
        }
    }
}
