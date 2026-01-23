using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Scriban;

namespace CateringEcommerce.BAL.Notification
{
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TemplateService> _logger;

        public TemplateService(
            ITemplateRepository repository,
            IMemoryCache cache,
            ILogger<TemplateService> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<string> RenderTemplateAsync(
            string templateCode,
            string language,
            Dictionary<string, object> data,
            CancellationToken cancellationToken = default)
        {
            // Get template from cache or database
            var template = await GetTemplateAsync(templateCode, language, cancellationToken);

            if (template == null)
            {
                _logger.LogWarning(
                    "Template {TemplateCode} not found for language {Language}",
                    templateCode, language);

                // Fallback to default language
                template = await GetTemplateAsync(templateCode, "en", cancellationToken);

                if (template == null)
                {
                    throw new TemplateNotFoundException(templateCode);
                }
            }

            // Render template using Scriban (or Liquid, Handlebars, etc.)
            var scribanTemplate = Template.Parse(template.Body);
            var rendered = await scribanTemplate.RenderAsync(data);

            // Update usage statistics
            await _repository.IncrementUsageCountAsync(template.TemplateId);

            return rendered;
        }

        private async Task<NotificationTemplate> GetTemplateAsync(
            string templateCode,
            string language,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"template:{templateCode}:{language}";

            if (_cache.TryGetValue(cacheKey, out NotificationTemplate cachedTemplate))
            {
                return cachedTemplate;
            }

            var template = await _repository.GetActiveTemplateAsync(
                templateCode,
                language,
                cancellationToken);

            if (template != null)
            {
                _cache.Set(cacheKey, template, TimeSpan.FromMinutes(30));
            }

            return template;
        }

        public async Task<string> RenderSubjectAsync(
            string templateCode,
            string language,
            Dictionary<string, object> data,
            CancellationToken cancellationToken = default)
        {
            var template = await GetTemplateAsync(templateCode, language, cancellationToken);

            if (template == null || string.IsNullOrEmpty(template.Subject))
            {
                return "Notification";
            }

            var scribanTemplate = Template.Parse(template.Subject);
            return await scribanTemplate.RenderAsync(data);
        }
    }
}
