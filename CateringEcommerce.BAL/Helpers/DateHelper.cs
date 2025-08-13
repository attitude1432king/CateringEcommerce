using System.Globalization;

namespace CateringEcommerce.BAL.Helpers
{
    public static class DateHelper
    {
        /// <summary>
        /// Converts a string in "yyyy-MM-dd" format to nullable DateTime.
        /// Returns null if the input is null, empty, or invalid.
        /// </summary>
        public static DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            if (DateTime.TryParseExact(
                    dateString,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedDate))
            {
                return parsedDate;
            }

            return null;
        }
    }
}
