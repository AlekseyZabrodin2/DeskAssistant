namespace DeskAssistant.Services
{
    public class EmailSettings
    {
        public string EmailId { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseTLS { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
