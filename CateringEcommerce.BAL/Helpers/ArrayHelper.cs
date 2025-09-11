using System.Text.Json;

namespace CateringEcommerce.BAL.Helpers
{
    public class ArrayHelper
    {
        /// <summary>
        /// Converts a comma-separated string to an array of integers.
        /// Ignores empty or invalid entries.
        /// </summary>
        public static int[] ConvertStringToIntArray(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Array.Empty<int>();

            return input
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    int val;
                    return int.TryParse(s.Trim(), out val) ? (int?)val : null;
                })
                .Where(i => i.HasValue)
                .Select(i => i.Value)
                .ToArray();
        }

        /// <summary>
        /// Converts a JSON array string (e.g. '["A","B","C"]') to a string array.
        /// Returns an empty array if input is null, empty, or invalid.
        /// </summary>
        public static string[] ConvertJsonStringToStringArray(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return Array.Empty<string>();

            try
            {
                return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
            }
            catch
            {
                // If not a valid JSON array, fallback: try comma-separated
                return json.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(s => s.Trim())
                           .Where(s => !string.IsNullOrEmpty(s))
                           .ToArray();
            }
        }
    }
}
