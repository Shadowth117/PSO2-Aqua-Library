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

        public static int NOF0Append(List<int> nof0, int currentOffset, int countToCheck = 1, int subtractedOffset = 0)
        {
            if (countToCheck < 1)
            {
                return -1;
            }
            int newAddress = currentOffset - subtractedOffset;
            nof0.Add(newAddress);

            return newAddress;
        }

        public static void AddOntoDict(Dictionary<string, List<int>> dict, List<string> strList, string str, int address)
        {
            str = str ?? "";
            if (dict.ContainsKey(str))
            {
                dict[str].Add(address);
            }
            else
            {
                strList.Add(str);
                dict.Add(str, new List<int>() { address });
            }
        }

        public static void AddNIFLText(int address, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList, string str)
        {
            NOF0Append(nof0PointerLocations, address, 1);
            AddOntoDict(textAddressDict, textList, str, address);
        }
    }
}
