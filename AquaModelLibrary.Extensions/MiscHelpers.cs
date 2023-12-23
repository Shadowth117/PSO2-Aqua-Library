using System.Numerics;
using System.Runtime.InteropServices;

namespace AquaModelLibrary.Helpers
{
    public class MiscHelpers
    {
        public static Vector2 UshortsToVector2(ushort[] ushorts)
        {
            return new Vector2((float)((double)ushorts[0] / ushort.MaxValue), (float)((double)ushorts[1] / ushort.MaxValue));
        }

        public static float[] VectorAsArray(Vector3 vec3)
        {
            return new float[] { vec3.X, vec3.Y, vec3.Z };
        }

        public static bool IsEqualByteArray(byte[] bArr0, byte[] bArr1)
        {
            if (bArr0.Length != bArr1.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < bArr0.Length; i++)
                {
                    if (bArr0[i] != bArr1[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsEqualShortArray(short[] sArr0, short[] sArr1)
        {
            if (sArr0.Length != sArr1.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < sArr0.Length; i++)
                {
                    if (sArr0[i] != sArr1[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public List<Vector2> getUVFlipped(List<Vector2> uvList)
        {
            List<Vector2> uvs = uvList.ToList();

            for (int i = 0; i < uvs.Count; i++)
            {
                Vector2 uv = uvs[i];
                uv.Y = -uv.Y;
                uvs[i] = uv;
            }

            return uvs;
        }

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
