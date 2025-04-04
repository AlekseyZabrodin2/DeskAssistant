namespace DeskAssistant.Services
{
    public interface IEmailService
    {
        bool SendEmail(string mailSubscriberTo, string emailIdTo, string emailSubject, string emailTextBody);
    }
}
