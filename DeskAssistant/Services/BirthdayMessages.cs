namespace DeskAssistant.Services
{
    public class BirthdayMessages
    {
        private readonly List<string> _messages = new()
        {
            "Наш коллега — {name} {lastname} празднует день рождения {date}! Пожелаем счастья, здоровья и отличного настроения! 🎉",
            "{name} {lastname} празднует день рождения ({date})! Не забудьте поздравить и пожелать побольше позитива и вдохновения! 🎂",
            "Друзья, наш цифровой коллега {name} {lastname} празднует день рождения ({date})! 🎈 Пусть этот день будет наполнен радостью и добрыми словами.",
            "{date} родился на товарищ, коллега, друг {name} {lastname}! Так что не забудьте поздравить и пожелать кучу счастья и успеха. 🥳",
            "Внимание, внимание! 🚨 {date} - празднует день рождения {name} {lastname}! Пусть этот день будет наполнен морем приятных эмоций. 🎈"
        };

        private readonly Random _random = new();

        public string GetRandomMessage(string name, string lastName, DateTime date)
        {
            if (_messages.Count == 0)
                return $"Не забудьте поздравить {name} {lastName} с днём рождения {date}! 🎉";

            var template = _messages[_random.Next(_messages.Count)];

            return template
                .Replace("{name}", name)
                .Replace("{lastname}", lastName)
                .Replace("{date}", date.ToString("dd.MM"));
        }
    }
}
