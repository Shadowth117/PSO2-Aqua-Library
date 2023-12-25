using System.Runtime.InteropServices;

namespace AquaModelLibrary.Data.DataTypes.SetLengthStrings
{
    public unsafe class SetLengthHelper
    {
        public static string GetPSO2String(byte* str, int end)
        {
            string finalText;

            byte[] text = new byte[end];
            Marshal.Copy(new IntPtr(str), text, 0, end);
            finalText = System.Text.Encoding.UTF8.GetString(text);

            return finalText;
        }
    }
}
