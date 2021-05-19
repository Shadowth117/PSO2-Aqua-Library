using NvTriStripDotNet;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary
{
    public unsafe class NGSAquaObject : AquaObject
    {
        public enum NGSVertFlags : int
        {
            VertPosition = 0x0,
            VertWeight = 0x1,
            VertNormal = 0x2,
            VertColor = 0x3,
            VertColor2 = 0x4,
            VertWeightIndex = 0xb,
            VertUV1 = 0x10,
            VertUV2 = 0x11,
            VertUV3 = 0x12,
            VertUV4 = 0x13,
            VertTangent = 0x20,
            VertBinormal = 0x21,
            Vert0x22 = 0x22,
            Vert0x23 = 0x23
        }

        public static Dictionary<int, int> NGSVertDataTypes = new Dictionary<int, int>()
        {
            { (int)NGSVertFlags.VertPosition, 0x3 }, //(0x0 Vertex Position) 
            { (int)NGSVertFlags.VertWeight, 0x11 }, //(0x1 Vertex Weights)
            { (int)NGSVertFlags.VertNormal, 0xF }, //(0x2 Vertex Normal)
            { (int)NGSVertFlags.VertColor, 0x5 }, //(0x3 Vertex Color)
            { (int)NGSVertFlags.VertColor2, 0x5 }, //(0x4 Vertex Color2)
            { (int)NGSVertFlags.VertWeightIndex, 0x7 }, //(0xb Weight Index)
            { (int)NGSVertFlags.VertUV1, 0xE }, //(0x10 UV1 Buffer)
            { (int)NGSVertFlags.VertUV2, 0x2 }, //(0x11 UV2 Buffer)
            { (int)NGSVertFlags.VertUV3, 0x2 }, //(0x12 UV3 Buffer)
            { (int)NGSVertFlags.VertUV4, 0x2 }, //(0x13 UV4 Buffer)
            { (int)NGSVertFlags.VertTangent, 0xF }, //(0x20 Tangents)
            { (int)NGSVertFlags.VertBinormal, 0xF }, //(0x21 Binormals)
            { (int)NGSVertFlags.Vert0x22, 0xC}, //(0x22 VertUnk0)
            { (int)NGSVertFlags.Vert0x23, 0xC}  //(0x23 VertUnk1)
        };

        //Same as pso2 equivalent, but the 2 parts at the end are actually used for offsets to 2 new structs
        public class NGSSHAD : SHAD
        {
            public SHADDetail shadDetail;
            public List<SHADExtraEntry> shadExtra = new List<SHADExtraEntry>();
        }

        public static NGSSHAD ReadNGSSHAD(BufferedStreamReader streamReader, int offset)
        {
            NGSSHAD shad = new NGSSHAD();
            shad.unk0 = streamReader.Read<int>();
            shad.pixelShader = streamReader.Read<PSO2String>();
            shad.vertexShader = streamReader.Read<PSO2String>();
            shad.shadDetailOffset = streamReader.Read<int>();
            shad.shadExtraOffset = streamReader.Read<int>();

            long bookmark = streamReader.Position();
            //Some shaders, like some player ones apparently, do not use the extra structs...
            if (shad.shadDetailOffset > 0)
            {
                streamReader.Seek(shad.shadDetailOffset + offset, System.IO.SeekOrigin.Begin);
                shad.shadDetail = streamReader.Read<SHADDetail>();

                streamReader.Seek(shad.shadExtraOffset + offset, System.IO.SeekOrigin.Begin);
                for(int i = 0; i < shad.shadDetail.shadExtraCount; i++)
                {
                    shad.shadExtra.Add(streamReader.Read<SHADExtraEntry>());
                }
            }
            else if (shad.shadExtraOffset > 0)
            {
                Console.WriteLine("**Apparently shadExtraOffset is allowed to be used without shadDetailOffset???**");
            }
            streamReader.Seek(bookmark, System.IO.SeekOrigin.Begin);

            return shad;
        }

        //Struct containing details for the shadExtra area, including a count needed to read it.
        public struct SHADDetail
        {
            public int unk0;
            public int shadExtraCount; //Details how many entries will be in the associated shadExtra
            public int unk1;
            public int unkCount0; //A count for something within the shadExtra area, presumably

            public int unk2;
            public int unkCount1; //Another count. Seemingly shadExtraCount - unkCount0; the other types of entries in the shadExtra
            public int unk3;
            public int unk4;
        }

        //Contains strings referencing shader information 
        public struct SHADExtraEntry
        {
            public short entryFlag0;
            public NGSShaderString entryString;
            public short entryFlag1;
            public short entryFlag2;

            public Vector4 entryFloats;
        }
    }
}
