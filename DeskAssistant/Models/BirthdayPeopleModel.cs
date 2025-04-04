using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskAssistant.Models
{
    public partial class BirthdayPeopleModel
    {

        public int Id { get; set; }

        public string LastName { get; set; }

        public string Name { get; set; }

        public string MiddleName { get; set; }

        public DateTime Birthday { get; set; }

        public DateTime BirthdayFormatted => new (Birthday.Year, Birthday.Month, Birthday.Day);

        public string Email { get; set; }

    }
}
