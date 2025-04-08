using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeskAssistant.Models;
using DeskAssistant.Services;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.DependencyInjection;
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

        public List<(string Name, string Address)> AllAddressees { get; set; }



        public void Initialize()
        {
            string filePath = "Data\\SpisokORPK.docx";
            ReadTextFromDocx(Path.Combine(AppContext.BaseDirectory, filePath));

            GetPeoplesWithBirthdayInMonth();
        }



        [RelayCommand]
        private void SendEmail()
        {
            var recipientsList = GetAddresseesList();

            SendMessagesRecipientsWithoutBirthday(recipientsList);
        }

        public void SendMessagesRecipientsWithoutBirthday(List<(string Name, string Address)> recipientsList)
        {

            foreach (var personWithBirthday in PeoplesWithBirthday)
            {
                var recipientName = $"{personWithBirthday.LastName} {personWithBirthday.Name}";

                var recipients = recipientsList.Where(p => p.Name != recipientName).ToList();

                var subject = "Напоминание о дне рождения 🎉";
                var messageBody = _birthdayMessages.GetRandomMessage(personWithBirthday.Name, personWithBirthday.LastName, personWithBirthday.Birthday);

                if (recipients.Any())
                    _emailService.SendEmail(recipients, subject, messageBody);
            }
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


            /// Get 1 Birthday People

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



    }
}
