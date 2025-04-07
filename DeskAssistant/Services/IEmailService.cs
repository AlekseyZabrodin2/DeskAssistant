namespace DeskAssistant.Services
{
    public interface IEmailService
    {
        bool SendEmail(List <(string nameTo, string emailTo)> addresse, string emailTextBody, string attachmentFile);
    }
}
