namespace CateringEcommerce.BAL.Helpers
{
    public static class BoolExtension
    {
        /// <summary>
        /// Converts a boolean value to 1 (true) or 0 (false).
        /// </summary>
        /// <param name="value">The boolean value.</param>
        /// <returns>1 if true, 0 if false.</returns>
        public static int ToBinary(this bool value)
        {
            return value ? 1 : 0;
        }
    }

}
