using System.Security.Cryptography;
using System.Text;

namespace AquaModelLibrary.Helpers
{
    public class HashHelpers
    {
        //Toggle to enable console version support
        public static bool useFileNameHash = true;

        public static string GetFileHash(string str)
        {
            if (!useFileNameHash)
            {
                return str;
            }

            if (str == null)
            {
                return "";
            }
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(new UTF8Encoding().GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public static string GetFileDataHash(string fileName)
        {
            if (fileName == null)
            {
                return "";
            }
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(File.ReadAllBytes(fileName));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public static string GetRebootHash(string fileName)
        {
            if (!useFileNameHash)
            {
                return fileName;
            }

            return fileName.Substring(0, 2) + "\\" + fileName.Substring(2, fileName.Length - 2);
        }
    }
}
