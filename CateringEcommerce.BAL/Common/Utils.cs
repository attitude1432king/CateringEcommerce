using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CateringEcommerce.BAL.Common
{
    public static class Utils
    {
        /// <summary>
        /// Gete a random OTP (One Time Password) of specified length.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateOtp(int length = 6)
        {
            var random = new Random();
            var otp = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                otp.Append(random.Next(0, 10));
            }

            return otp.ToString();
        }

        public static string GetEnumDisplayName(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
                                  .FirstOrDefault() as DisplayAttribute;

            return attribute?.Name ?? value.ToString();
        }

    }
}
