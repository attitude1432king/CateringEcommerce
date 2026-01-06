using System.Data;

namespace CateringEcommerce.BAL.Helpers
{
    public static class DataRowExtensions
    {
        /// <summary>
        /// Gets a boolean value from the DataRow column, supporting values like 1, 0, Yes, No, True, False.
        /// </summary>
        public static bool GetBoolean(this DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                throw new ArgumentException($"Column '{columnName}' does not exist.");

            var value = row[columnName];

            if (value == null || value == DBNull.Value)
                return false;

            string str = value.ToString().Trim().ToLower();

            return str switch
            {
                "1" => true,
                "yes" => true,
                "true" => true,
                "y" => true,
                "0" => false,
                "no" => false,
                "false" => false,
                "n" => false,
                _ => false  // You can throw here instead if unknown value
            };
        }

        /// <summary>
        /// Safely gets a column value from DataRow.
        /// Returns default(T) if value is DBNull, null, or empty.
        /// </summary>
        public static T GetValue<T>(this DataRow row, string columnName, T defaultValue = default!)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
                return defaultValue;

            object value = row[columnName];

            if (value == DBNull.Value || value == null)
                return defaultValue;

            // Handle string
            if (typeof(T) == typeof(string))
            {
                var str = value.ToString();
                return string.IsNullOrWhiteSpace(str)
                    ? defaultValue
                    : (T)(object)str;
            }

            // Handle enum
            if (typeof(T).IsEnum)
            {
                try
                {
                    return (T)Enum.ToObject(typeof(T), value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            try
            {
                return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
