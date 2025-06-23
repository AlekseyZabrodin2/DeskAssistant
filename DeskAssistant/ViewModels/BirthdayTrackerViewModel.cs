using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeskAssistant.Models;
using DeskAssistant.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace DeskAssistant.ViewModels
{
    public partial class BirthdayTrackerViewModel : ObservableObject
    {

        public EmailService _emailService;
        IServiceProvider _serviceProvider;
        public BirthdayMessages _birthdayMessages = new();


        public BirthdayTrackerViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _emailService = _serviceProvider.GetRequiredService<EmailService>();

            Initialize();
        }


        public ObservableCollection<BirthdayPeopleModel> BirthdayPeoples { get; set; }

        public ObservableCollection<BirthdayPeopleModel> PeoplesWithBirthday { get; set; }

        public BirthdayPeopleModel PeopleNextBirthday { get; set; }

        public List<(string Name, string Address)> AllAddressees { get; set; }



        public void Initialize()
        {
            string filePath = "Data\\SpisokORPK.docx";
            ReadTextFromDocx(Path.Combine(AppContext.BaseDirectory, filePath));

            GetPeoplesWithBirthdayInMonth();
        }

        public void ReadTextFromDocx(string filePath)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                var textList = wordDoc.MainDocumentPart.Document.Body
                    .Descendants<Paragraph>()
                    .Where(p => !string.IsNullOrWhiteSpace(p.InnerText))
                    .Select(p => p.InnerText.Trim())
                    .ToList();

                GetBirthdayPeoples(textList);
            }
        }

        public ObservableCollection<BirthdayPeopleModel> GetBirthdayPeoples(List<string> textList)
        {
            var birthdayPeoples = new ObservableCollection<BirthdayPeopleModel>();

            var birthdayPeoplesCount = 1;

            for (int i = 0; i < textList.Count; i += 3)
            {
                var fullName = textList[i].Trim().Split(' ');

                var lastname = fullName[0];
                var name = fullName[1];
                var middleName = fullName.Length > 2 ? fullName[2] : "";

                var birthday = textList[i + 1];

                var email = textList[i + 2];

                birthdayPeoples.Add(new BirthdayPeopleModel
                {
                    Id = birthdayPeoplesCount++,
                    LastName = lastname,
                    Name = name,
                    MiddleName = middleName,
                    Birthday = DateTime.Parse(birthday),
                    Email = email
                });
            }

            BirthdayPeoples = birthdayPeoples;

            return BirthdayPeoples;
        }

        public List<(string Name, string Address)> GetAddresseesList()
        {
            var allAddressees = new List<(string Name, string Address)>();

            foreach (var people in BirthdayPeoples)
            {
                var name = $"{people.LastName} {people.Name}";
                var email = people.Email;

                allAddressees.Add((name, email));
            }

            AllAddressees = allAddressees;

            return AllAddressees;
        }

        public ObservableCollection<BirthdayPeopleModel> GetPeoplesWithBirthdayInMonth()
        {
            var peoplesWithBirthday = new ObservableCollection<BirthdayPeopleModel>();
            var today = DateTime.Today;


            /// Get 1 Birthday People today

            //var peoples = BirthdayPeoples
            //    .Where(p => p.Birthday.Month == today.Month && p.Birthday.Day == today.Day)
            //    .OrderBy(p => p.Birthday.Day)
            //    .ToList();


            var peoples = BirthdayPeoples
                .Where(p => p.Birthday.Month == today.Month && p.Birthday.Day >= today.Day)
                .OrderBy(p => p.Birthday.Day)
                .ToList();

            int idCounter = 1;

            foreach (var person in peoples)
            {
                peoplesWithBirthday.Add(new BirthdayPeopleModel
                {
                    Id = idCounter++,
                    LastName = person.LastName,
                    Name = person.Name,
                    MiddleName = person.MiddleName,
                    Birthday = person.Birthday,
                    IsBirthdayThisMonth = true,
                    Email = person.Email
                });
            }

            PeoplesWithBirthday = peoplesWithBirthday;

            return PeoplesWithBirthday;
        }

        public BirthdayPeopleModel GetPersonWithNextBirthday()
        {
            var today = DateTime.Today;

            if (BirthdayPeoples.Any())
            {
                var person = BirthdayPeoples
                .Where(p => p.Birthday.Month == today.Month && p.Birthday.Day >= today.Day)
                .OrderBy(p => p.Birthday.Day)
                .FirstOrDefault();

                if (person == null)
                    return null;

                PeopleNextBirthday = new()
                {
                    Id = 1,
                    LastName = person.LastName,
                    Name = person.Name,
                    MiddleName = person.MiddleName,
                    Birthday = person.Birthday,
                    IsBirthdayThisMonth = true,
                    Email = person.Email
                };
            }

            return PeopleNextBirthday;
        }




        [RelayCommand]
        private async Task SendEmailAboutAllBirthdays()
        {
            var recipientsList = GetAddresseesList();

            await SendMessagesRecipientsWithoutBirthday(recipientsList);
        }

        public async Task SendMessagesRecipientsWithoutBirthday(List<(string Name, string Address)> recipientsList)
        {
            if (PeoplesWithBirthday.Any())
            {
                foreach (var personWithBirthday in PeoplesWithBirthday)
                {
                    var recipientName = $"{personWithBirthday.LastName} {personWithBirthday.Name}";

                    var recipients = recipientsList.Where(p => p.Name != recipientName).ToList();

                    var subject = "Напоминание о дне рождения 🎉";
                    var messageBody = _birthdayMessages.GetRandomMessage(personWithBirthday.Name, personWithBirthday.LastName, personWithBirthday.Birthday);

                    if (recipients.Any())
                        await _emailService.SendEmailAsync(recipients, subject, messageBody);
                }
            }
            else
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = App.MainWindow.Content.XamlRoot,
                    Title = "Нет именинников",
                    Content = "В этом месяце больше никто не празднует день рождения.",
                    CloseButtonText = "ОК"

                };

                await dialog.ShowAsync();
            }
        }


        [RelayCommand]
        private async Task SendEmailAboutNextBirthday()
        {
            var recipientsList = GetAddresseesList();
            GetPersonWithNextBirthday();

            await SendEmailRecipientsAboutNextBirthday(recipientsList);
        }

        private async Task SendEmailRecipientsAboutNextBirthday(List<(string Name, string Address)> recipientsList)
        {
            if (PeopleNextBirthday != null)
            {
                var recipientName = $"{PeopleNextBirthday.LastName} {PeopleNextBirthday.Name}";

                var recipients = recipientsList.Where(p => p.Name != recipientName).ToList();

                var subject = "Напоминание о дне рождения 🎉";
                var messageBody = _birthdayMessages.GetRandomMessage(PeopleNextBirthday.Name, PeopleNextBirthday.LastName, PeopleNextBirthday.Birthday);
                var finalMessage = $"{messageBody}" + "\r\nПодходите поздравляйте, не стесняйтесь";

                if (recipients.Any())
                    await _emailService.SendEmailAsync(recipients, subject, finalMessage);
            }
            else
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = App.MainWindow.Content.XamlRoot,
                    Title = "Нет именинников",
                    Content = "В этом месяце больше никто не празднует день рождения.",
                    CloseButtonText = "ОК"

                };

                await dialog.ShowAsync();
            }
        }


    }
}
