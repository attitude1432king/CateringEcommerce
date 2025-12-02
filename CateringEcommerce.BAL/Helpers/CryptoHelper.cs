using System.Security.Cryptography;
using System.Text;

namespace CateringEcommerce.BAL.Helpers
{
    public static class CryptoHelper
    {
        // Converts any custom key into a valid 32-byte AES key
        private static byte[] GetAesKey(string key)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(key)); // 256-bit key
            }
        }

        public static string Encrypt(string plainText, string customKey)
        {
            byte[] keyBytes = GetAesKey(customKey);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.GenerateIV(); // random IV (safer)

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // prepend IV

                    using (CryptoStream cs = new CryptoStream(
                        ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(inputBytes, 0, inputBytes.Length);
                    }

                    return ToUrlSafeBase64(Convert.ToBase64String(ms.ToArray()));

                }
            }
        }

        public static string Decrypt(string cipherText, string customKey)
        {
            cipherText = FromUrlSafeBase64(cipherText);
            byte[] fullCipher = Convert.FromBase64String(cipherText);
            byte[] keyBytes = GetAesKey(customKey);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;

                // Extract IV (first 16 bytes)
                byte[] iv = new byte[aes.BlockSize / 8];
                byte[] cipherBytes = new byte[fullCipher.Length - iv.Length];

                Array.Copy(fullCipher, iv, iv.Length);
                Array.Copy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(
                    ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.FlushFinalBlock();

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypts a cipherText using the customKey and converts it to type T.
        /// Throws an exception if decryption or conversion fails.
        /// </summary>
        public static T DecryptAndConvert<T>(string cipherText, string customKey)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                throw new ArgumentException("Cipher text cannot be null or empty.", nameof(cipherText));

            string decryptedText;

            try
            {
                decryptedText = Decrypt(cipherText, customKey);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Decryption failed.", ex);
            }

            try
            {
                // Convert to target type
                return (T)Convert.ChangeType(decryptedText, typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    $"Decrypted value '{decryptedText}' cannot be converted to {typeof(T).Name}.", ex);
            }
        }

        private static string ToUrlSafeBase64(string input)
        {
            return input.Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static string FromUrlSafeBase64(string input)
        {
            input = input.Replace("-", "+").Replace("_", "/");
            switch (input.Length % 4)
            {
                case 2: input += "=="; break;
                case 3: input += "="; break;
            }
            return input;
        }
    }
}
