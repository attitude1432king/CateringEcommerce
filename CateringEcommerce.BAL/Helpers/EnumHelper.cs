using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CateringEcommerce.BAL.Helpers
{
    /// <summary>
    /// Utility class for working with enums that have Display attributes
    /// Provides conversion between enum values, int codes, and display names
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Gets the Display Name attribute value for an enum value
        /// Example: ApprovalStatus.Pending → "Pending"
        /// </summary>
        public static string GetDisplayName<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? enumValue.ToString();
        }

        /// <summary>
        /// Converts an integer value to its enum display name
        /// Example: 1 → "Pending" (for ApprovalStatus)
        /// </summary>
        public static string GetDisplayNameFromInt<TEnum>(int value) where TEnum : Enum
        {
            if (Enum.IsDefined(typeof(TEnum), value))
            {
                var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), value);
                return GetDisplayName(enumValue);
            }
            return "Unknown";
        }

        /// <summary>
        /// Converts an integer value to enum
        /// Example: 1 → ApprovalStatus.Pending
        /// </summary>
        public static TEnum? GetEnumFromInt<TEnum>(int value) where TEnum : struct, Enum
        {
            if (Enum.IsDefined(typeof(TEnum), value))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), value);
            }
            return null;
        }

        /// <summary>
        /// Converts enum to its integer value
        /// Example: ApprovalStatus.Pending → 1
        /// </summary>
        public static int GetIntValue<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            return Convert.ToInt32(enumValue);
        }

        /// <summary>
        /// Validates if an integer is a valid enum value
        /// </summary>
        public static bool IsValidEnumValue<TEnum>(int value) where TEnum : Enum
        {
            return Enum.IsDefined(typeof(TEnum), value);
        }

        /// <summary>
        /// Gets all enum values as a dictionary (int value → display name)
        /// Useful for dropdown lists in UI
        /// </summary>
        public static Dictionary<int, string> GetEnumDictionary<TEnum>() where TEnum : Enum
        {
            var dictionary = new Dictionary<int, string>();
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                dictionary.Add(Convert.ToInt32(value), GetDisplayName(value));
            }
            return dictionary;
        }
    }

    /// <summary>
    /// Extension methods for Enum types
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Extension method to get display name from an enum value
        /// Usage: DocumentType.Kitchen.GetDisplayName()
        /// </summary>
        public static string GetDisplayName(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}
