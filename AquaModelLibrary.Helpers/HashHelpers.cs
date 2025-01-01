using System.Security.Cryptography;
using System.Text;

namespace AquaModelLibrary.Helpers
{
    public class HashHelpers
    {
        //Toggle to enable console version support
        public static bool useFileNameHash = true;

        /// <summary>
        /// Retrieves final PSO2 filename given a string. If useFileNameHash is false, the string is returned as is for handling console versions.
        /// The hash itself is standard MD5 and gets returned as a string sans delimiters.
        /// </summary>
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

        /// <summary>
        /// Gets an MD5 hash of a file given a filepath. Returns an empty string if the file path is null or doesn't exist.
        /// </summary>
        public static string GetFileDataHash(string fileName)
        {
            if (fileName == null || !File.Exists(fileName))
            {
                return "";
            }
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(File.ReadAllBytes(fileName));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        /// <summary>
        /// Separates the first 2 characters of the string as a directory, as PSO2: NGS does for its paths.
        /// </summary>
        public static string GetRebootHash(string fileName)
        {
            if (!useFileNameHash)
            {
                return fileName;
            }

            return fileName.Substring(0, 2) + "\\" + fileName.Substring(2, fileName.Length - 2);
        }


        /// <summary>]
        /// Get the hash of a string, <paramref name="utf8Str"/> must be lowercased
        /// Specific hash is MurmurHash2, 64 bit.
        /// Sourced from https://github.com/aianlinb/VisualGGPK2
        /// </summary>
        public static unsafe ulong MurmurHash64A(ReadOnlySpan<byte> utf8Str, ulong seed = 0x1337B33F)
        {
            if (utf8Str[^1] == '/') // TrimEnd('/')
                utf8Str = utf8Str[..^1];
            var length = utf8Str.Length;

            const ulong m = 0xC6A4A7935BD1E995UL;
            const int r = 47;

            ulong h = seed ^ ((ulong)length * m);

            fixed (byte* data = utf8Str)
            {
                ulong* p = (ulong*)data;
                int numberOfLoops = length >> 3; // div 8
                while (numberOfLoops-- != 0)
                {
                    ulong k = *p++;
                    k *= m;
                    k ^= k >> r;
                    k *= m;

                    h ^= k;
                    h *= m;
                }

                int remainingBytes = length & 0b111; // mod 8
                if (remainingBytes != 0)
                {
                    int offset = (8 - remainingBytes) << 3; // mul 8
                    h ^= *p & (0xFFFFFFFFFFFFFFFFUL >> offset);
                    h *= m;
                }
            }

            h ^= h >> r;
            h *= m;
            h ^= h >> r;

            return h;
        }

        /// <summary>
        /// Get the hash of a string 
        /// Used for POE builds before patch 3.21.2
        /// Specific hash is FNV-1a 64 bit.
        /// Sourced from https://github.com/aianlinb/VisualGGPK2
        /// </summary>
        public static unsafe ulong FNV1a64Hash(string str)
        {
            byte[] b;
            if (str.EndsWith('/'))
                b = Encoding.UTF8.GetBytes(str, 0, str.Length - 1);
            else
                b = Encoding.UTF8.GetBytes(str.ToLowerInvariant());

            // Equals to: b.Aggregate(0xCBF29CE484222325UL, (current, by) => (current ^ by) * 0x100000001B3UL);
            var hash = 0xCBF29CE484222325UL;
            foreach (var by in b)
                hash = (hash ^ by) * 0x100000001B3UL;
            return (((hash ^ 43) * 0x100000001B3UL) ^ 43) * 0x100000001B3UL; // "++"  -->  '+'==43
        }
    }
}
