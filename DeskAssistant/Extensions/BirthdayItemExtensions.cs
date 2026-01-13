using BirthdaysGrpcService;
using DeskAssistant.Core.Models;

namespace DeskAssistant.Extensions
{
    public class BirthdayItemExtensions
    {
        public BirthdaysEntity GrpcBirthdayItemToBirthdaysEntity(BirthdayItem birthdaysItem)
        {
            return new BirthdaysEntity
            {
                Id = string.IsNullOrEmpty(birthdaysItem.Id) ? 0 : int.Parse(birthdaysItem.Id),
                LastName = birthdaysItem.LastName,
                Name = birthdaysItem.Name,
                MiddleName = birthdaysItem.MiddleName,
                Birthday = birthdaysItem.Birthday == "" ? null : DateTime.SpecifyKind(DateTime.Parse(birthdaysItem.Birthday), DateTimeKind.Utc),
                Email = birthdaysItem.Email
            };
        }

        public BirthdayItem BirthdaysEntityToGrpcBirthdayItem(BirthdaysEntity birthdaysEntity)
        {
            return new BirthdayItem
            {
                Id = birthdaysEntity.Id.ToString(),
                LastName = birthdaysEntity.LastName,
                Name = birthdaysEntity.Name,
                MiddleName = birthdaysEntity.MiddleName,
                Birthday = birthdaysEntity.Birthday.ToString(),
                Email = birthdaysEntity.Email
            };
        }
    }
}
