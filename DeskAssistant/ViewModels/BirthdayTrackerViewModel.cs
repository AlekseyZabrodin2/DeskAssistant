using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeskAssistant.Models;
using DeskAssistant.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DeskAssistant.ViewModels
{
    public partial class BirthdayTrackerViewModel : ObservableObject
    {

        public EmailService _emailService;
        
        IServiceProvider _serviceProvider;

        public BirthdayTrackerViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _emailService = _serviceProvider.GetRequiredService<EmailService>();

            Initialize();
        }

        public ObservableCollection<BirthdayPeopleModel> BirthdayPeoples {  get; set; }



        public void Initialize()
        {
            string imagePath = @"D:\\Develop\\Tesseract\\Tesseract\\image\\SpisokORPK1.docx";

            ReadTextFromDocx(imagePath);
        }

        public void ReadTextFromDocx(string filePath)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                var birthdayPeoples = new ObservableCollection<BirthdayPeopleModel>();

                var textList = wordDoc.MainDocumentPart.Document.Body
                    .Descendants<Paragraph>()
                    .Where(p => !string.IsNullOrWhiteSpace(p.InnerText))
                    .Select(p => p.InnerText.Trim())
                    .ToList();

                for (int i = 0; i < textList.Count; i += 3)
                {
                    var id = i / 2 + 1;

                    var fullName = textList[i].Trim().Split(' ');

                    var lastname = fullName[0];
                    var name = fullName[1];
                    var middleName = fullName.Length > 2 ? fullName[2] : "";

                    var birthday = textList[i + 1];

                    var email = textList[i + 2];

                    birthdayPeoples.Add(new BirthdayPeopleModel 
                    {
                        Id = id, 
                        LastName = lastname, 
                        Name = name,
                        MiddleName = middleName,
                        Birthday = DateTime.Parse(birthday),
                        Email = email
                    });
                }

                BirthdayPeoples = birthdayPeoples;
            }
        }

        [RelayCommand]
        private void SendEmail()
        {
            foreach (var person in BirthdayPeoples)
            {
                var addressee = $"{person.LastName} {person.Name}";

                _emailService.SendEmail(addressee, person.Email,"Тестовое письмо", "Тестовые данные");
            }
        }



    }
}
