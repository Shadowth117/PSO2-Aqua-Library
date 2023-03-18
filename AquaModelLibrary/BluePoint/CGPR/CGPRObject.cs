using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.BluePoint.CAWS
{
    public abstract class CGPRObject
    {
        public uint magic;
        public int int_04;
        public int int_08;
        public int int_0C;

        public int int_10;
        public int int_14;

        //Ending bytes
        public int int_SemiFinal;
        public int int_Final;

        public CGPRObject()
        {

        }

        public CGPRObject(BufferedStreamReader sr)
        {
            magic = sr.Read<uint>();
        }
    }

    //Used commonly for CMDLs
    public class _00FAE885_Object : CGPRObject
    {
        public byte bt_18;
        public byte bt_19;
        public byte stringLengthPlus1;
        public byte stringLength;

        public int postStrInt_00;
        public int postStrInt_04;
        public int postStrInt_08;
        public int postStrInt_0C;

        public int postStrInt_10;
        public byte postStrBt_14;

        public int postStrInt_15;
        public int postStrInt_19;
        public int postStrInt_1D;
        public int postStrInt_21;

        public int postStrInt_25;

        public string cmdlPath = null;

        public _00FAE885_Object()
        {

        }

        public _00FAE885_Object(BufferedStreamReader sr)
        {
            magic = sr.Read<uint>();
            int_04 = sr.Read<int>();
            int_08 = sr.Read<int>();
            int_0C = sr.Read<int>();

            int_10 = sr.Read<int>();
            int_14 = sr.Read<int>();

            bt_18 = sr.Read<byte>();
            bt_19 = sr.Read<byte>();
            stringLengthPlus1 = sr.Read<byte>();
            stringLength = sr.Read<byte>();

            cmdlPath = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), stringLength));
            sr.Seek(stringLength, System.IO.SeekOrigin.Current);

            postStrInt_00 = sr.Read<int>();
            postStrInt_04 = sr.Read<int>();
            postStrInt_08 = sr.Read<int>();
            postStrInt_0C = sr.Read<int>();

            postStrInt_10 = sr.Read<int>();
            postStrBt_14 = sr.Read<byte>();

            postStrInt_15 = sr.Read<int>();
            postStrInt_19 = sr.Read<int>();
            postStrInt_1D = sr.Read<int>();
            postStrInt_21 = sr.Read<int>();

            postStrInt_25 = sr.Read<int>();

            int_SemiFinal = sr.Read<int>();
            int_Final = sr.Read<int>();
        }
    }

    public class _C1A69458_Object : CGPRObject
    {
        //Quaternions? Iunno
        public Vector4 vec4_0;
        public Vector4 vec4_1;
        public Vector4 vec4_2;

        public _C1A69458_Object()
        {

        }

        public _C1A69458_Object(BufferedStreamReader sr)
        {
            magic = sr.Read<uint>();
            int_04 = sr.Read<int>();
            int_08 = sr.Read<int>();
            int_0C = sr.Read<int>();

            int_10 = sr.Read<int>();
            int_14 = sr.Read<int>();
            
            vec4_0 = sr.Read<Vector4>();
            vec4_1 = sr.Read<Vector4>();
            vec4_2 = sr.Read<Vector4>();

            int_SemiFinal = sr.Read<int>();
            int_Final = sr.Read<int>();
        }
    }
}
