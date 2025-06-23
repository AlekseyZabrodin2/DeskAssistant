namespace DeskAssistant.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(List <(string nameTo, string emailTo)> addresse, string emailTextBody, string attachmentFile);
    }
}
