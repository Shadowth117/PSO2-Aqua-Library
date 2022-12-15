using NvTriStripDotNet;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
            Vert0x23 = 0x23,
            Vert0x24 = 0x24,
            Vert0x25 = 0x25,
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
            { (int)NGSVertFlags.Vert0x23, 0xC},  //(0x23 VertUnk1)
            { (int)NGSVertFlags.Vert0x24, 0xC}, //(0x24 VertUnk0)
            { (int)NGSVertFlags.Vert0x25, 0xC}  //(0x25 VertUnk1)
        };

        public struct unkStruct1
        {
            public int int_00;
            public int int_04;
            public int int_08;
            public int int_0C;
        }

        public static SHAD ReadNGSSHAD(BufferedStreamReader streamReader, int offset, bool isNGSShader)
        {
            SHAD shad = new SHAD();
            shad.isNGS = true;
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

        public static SHADDetail CreateDetail(int unk0, int shadExtraCount, int unk1, int unkCount0, int unk2, int unkCount1, int unk3, int unk4)
        {
            SHADDetail det = new SHADDetail();
            det.unk0 = unk0;
            det.shadExtraCount = shadExtraCount;
            det.unk1 = unk1;
            det.unkCount0 = unkCount0;
            det.unk2 = unk2;
            det.unkCount1 = unkCount1;
            det.unk3 = unk3;
            det.unk4 = unk4;

            return det;
        }

        public static SHADExtraEntry CreateExtra(short entryFlag0, string entryString, short entryFlag1, short entryFlag2, Vector4 entryFloats)
        {
            SHADExtraEntry ext = new SHADExtraEntry();
            ext.entryFlag0 = entryFlag0;
            ext.entryString.SetString(entryString);
            ext.entryFlag1 = entryFlag1;
            ext.entryFlag2 = entryFlag2;
            ext.entryFloats = entryFloats;

            return ext;
        }
        public static SHADExtraEntry CreateExtra(short entryFlag0, string entryString, short entryFlag1, short entryFlag2)
        {
            SHADExtraEntry ext = new SHADExtraEntry();
            ext.entryFlag0 = entryFlag0;
            ext.entryString.SetString(entryString);
            ext.entryFlag1 = entryFlag1;
            ext.entryFlag2 = entryFlag2;

            return ext;
        }

        public override AquaObject Clone()
        {
            NGSAquaObject aqp = new NGSAquaObject();
            aqp.afp = afp;
            aqp.objc = objc;
            aqp.vsetList = new List<VSET>(vsetList);
            aqp.vtxeList = vtxeList.ConvertAll(vtxe => vtxe.Clone()).ToList();
            aqp.vtxlList = vtxlList.ConvertAll(vtxl => vtxl.Clone()).ToList();
            aqp.psetList = new List<PSET>(psetList);
            aqp.meshList = new List<MESH>(meshList);
            aqp.mateList = new List<MATE>(mateList);
            aqp.rendList = new List<REND>(rendList);
            aqp.shadList = shadList.ConvertAll(shad => shad.Clone()).ToList();
            aqp.tstaList = new List<TSTA>(tstaList);
            aqp.tsetList = tsetList.ConvertAll(tset => tset.Clone()).ToList();
            aqp.texfList = new List<TEXF>(texfList);
            if(aqp.unrms != null)
            {
                aqp.unrms = unrms.Clone();
            }
            aqp.strips = strips.ConvertAll(stp => stp.Clone()).ToList();

            //*** 0xC33 only
            aqp.bonePalette = new List<uint>(bonePalette);

            //Unclear the purpose of these, but when present they have a smaller count than initial mesh and psets. 
            aqp.unkStruct1List = new List<unkStruct1>(unkStruct1List);
            aqp.mesh2List = new List<MESH>(mesh2List);
            aqp.pset2List = new List<PSET>(pset2List);
            aqp.strips2 = strips2.ConvertAll(stp => stp.Clone()).ToList();

            aqp.strips3Lengths = new List<int>(strips3Lengths);
            aqp.strips3 = strips3.ConvertAll(stp => stp.Clone()).ToList();
            aqp.unkPointArray1 = new List<Vector3>(unkPointArray1); //Noooooooo idea what these are. Count matches the strips3Lengths count
            aqp.unkPointArray2 = new List<Vector3>(unkPointArray2);
            //***

            aqp.applyNormalAveraging = applyNormalAveraging;

            //Custom model related data
            aqp.tempTris = tempTris.ConvertAll(tri => tri.Clone()).ToList();
            aqp.tempMats = tempMats.ConvertAll(mat => mat.Clone()).ToList();

            return aqp;
        }
    }
}
