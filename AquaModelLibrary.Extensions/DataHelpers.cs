using System.Runtime.InteropServices;

namespace AquaModelLibrary.Helpers
{
    public class DataHelpers
    {

        //This shouldn't be necessary, but library binding issues in maxscript necessitated it over the Reloaded.Memory implementation. System.Runtime.CompilerServices.Unsafe causes errors otherwise.
        //Borrowed from: https://stackoverflow.com/questions/42154908/cannot-take-the-address-of-get-the-size-of-or-declare-a-pointer-to-a-managed-t
        private static byte[] ConvertStruct<T>(ref T str) where T : struct
        {
            int size = Marshal.SizeOf(str);
            IntPtr arrPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, arrPtr, true);
            var arr = new byte[size];
            Marshal.Copy(arrPtr, arr, 0, size);
            Marshal.FreeHGlobal(arrPtr);
            return arr;
        }

        public static byte[] ConvertStruct<T>(T str) where T : struct
        {
            return ConvertStruct(ref str);
        }
    }
}
