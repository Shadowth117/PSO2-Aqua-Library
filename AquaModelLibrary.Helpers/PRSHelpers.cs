using prs_rs.Net.Sys;

namespace AquaModelLibrary.Helpers
{
    public unsafe static class PRSHelpers
    {
        public static byte[] PRSCompress(byte[] bytes)
        {
            fixed (byte* srcPtr = bytes.ToArray())
            {
                // Get the maximum possible size of the compressed data
                nuint maxCompressedSize = NativeMethods.prs_calculate_max_compressed_size((nuint)bytes.Length);
                byte[] dest = GC.AllocateUninitializedArray<byte>((int)maxCompressedSize);
                fixed (byte* destPtr = &dest[0])
                {
                    nuint compressedSize = NativeMethods.prs_compress(srcPtr, destPtr, (nuint)bytes.Length);
                    var compressedBytes = new byte[compressedSize];
                    Array.Copy(dest, compressedBytes, (int)compressedSize);
                    return compressedBytes;
                }
            }
        }

        public static byte[] PRSDecompress(byte[] bytes)
        {
            // Calculate the decompressed size to allocate enough memory
            fixed (byte* srcPtr = bytes)
            {
                // or get from file header etc.
                nuint decompressedSize = NativeMethods.prs_calculate_decompressed_size(srcPtr);
                byte[] dest = GC.AllocateUninitializedArray<byte>((int)decompressedSize);
                fixed (byte* destPtr = &dest[0])
                {
                    nuint actualDecompressedSize = NativeMethods.prs_decompress(srcPtr, destPtr);
                    var decompressedBytes = new byte[actualDecompressedSize];
                    Array.Copy(dest, decompressedBytes, (int)actualDecompressedSize);
                    return decompressedBytes;
                }
            }
        }
    }
}
