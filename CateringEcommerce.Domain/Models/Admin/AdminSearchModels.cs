namespace CateringEcommerce.Domain.Models.Admin
{
    public class GlobalSearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int MaxResultsPerModule { get; set; } = 5;
    }

    public class GlobalSearchResultItem
    {
        public string Type { get; set; } = string.Empty;
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = "gray";
        public string ViewUrl { get; set; } = string.Empty;
        public string ModuleLabel { get; set; } = string.Empty;
        public string MatchedOn { get; set; } = string.Empty;
        public Dictionary<string, object?> ExtraData { get; set; } = new();
    }

    public class GlobalSearchResponse
    {
        public List<GlobalSearchResultItem> Results { get; set; } = new();
        public int TotalCount { get; set; }
        public string Query { get; set; } = string.Empty;
        public List<string> PermissionsUsed { get; set; } = new();
    }
}
