namespace CateringEcommerce.Domain.Interfaces.Notification
{
    public interface ITemplateService
    {
        Task<string> RenderTemplateAsync(string templateCode, string language, Dictionary<string, object> data, CancellationToken cancellationToken = default);
        Task<string> RenderSubjectAsync(string templateCode, string language, Dictionary<string, object> data, CancellationToken cancellationToken = default);
    }
}
