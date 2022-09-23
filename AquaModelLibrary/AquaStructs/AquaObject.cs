using NvTriStripDotNet;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static AquaModelLibrary.NGSAquaObject;
using AquaModelLibrary.Extra;
using System.IO;

namespace AquaModelLibrary

{    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe abstract class AquaObject : AquaCommon
    {
        public AquaPackage.AFPBase afp;
        public OBJC objc;
        public List<VSET> vsetList = new List<VSET>();
        public List<VTXE> vtxeList = new List<VTXE>();
        public List<VTXL> vtxlList = new List<VTXL>();
        public List<PSET> psetList = new List<PSET>();
        public List<MESH> meshList = new List<MESH>();
        public List<MATE> mateList = new List<MATE>();
        public List<REND> rendList = new List<REND>();
        public List<SHAD> shadList = new List<SHAD>();
        public List<TSTA> tstaList = new List<TSTA>();
        public List<TSET> tsetList = new List<TSET>();
        public List<TEXF> texfList = new List<TEXF>();
        public UNRM unrms;
        public List<stripData> strips = new List<stripData>();

        //*** 0xC33 only
        public List<uint> bonePalette = new List<uint>();

        //Unclear the purpose of these, but when present they have a smaller count than initial mesh and psets. 
        public List<unkStruct1> unkStruct1List = new List<unkStruct1>();
        public List<MESH> mesh2List = new List<MESH>();
        public List<PSET> pset2List = new List<PSET>();
        public List<stripData> strips2 = new List<stripData>(); //Strip set 2 is from the same array as the first, just split differently, potentially.

        public List<int> strips3Lengths = new List<int>();
        public List<stripData> strips3 = new List<stripData>();
        public List<Vector3> unkPointArray1 = new List<Vector3>(); //Noooooooo idea what these are. Count matches the strips3Lengths count
        public List<Vector3> unkPointArray2 = new List<Vector3>();
        //***

        public bool applyNormalAveraging = false;

        //Custom model related data
        public List<GenericTriangles> tempTris = new List<GenericTriangles>();
        public List<GenericMaterial> tempMats = new List<GenericMaterial>();

        //Credit to MiscellaneousModder for this list:
        public enum baseWearIds : int
        {
            costume = 0x0,
            breastNeck = 0x1,
            front = 0x2,
            accessory1 = 0x3,
            back = 0x4,
            shoulder = 0x5,
            foreArm = 0x6,
            leg = 0x7,
            accessory2 = 0x8,
            headOrnament = 0x9,
            castBodyOrnament = 0xA,
            castLegsOrnament = 0xB,
            castArmsOrnament = 0xC,
            outerOrnament = 0xD,
        }

        public struct BoundingVolume
        {
            public Vector3 modelCenter; //0x1E, Type 0x4A, Count 0x1
            public float reserve0;
            public float boundingRadius; //0x1F, Type 0xA                    //Distance of furthest point from the origin
            public Vector3 modelCenter2; //0x20, Type 0x4A, Count 0x1        //Model Center... again 
            public float reserve1;
            public Vector3 halfExtents; //0x21, Type 0x4A, Count 0x1 //Distance between max/min of x, y and z divided by 2
        }

        public struct OBJC
        {
            public int type;           //0x10, Type 0x8
            public int size;           //0x11, Type 0x8
            public int unkMeshValue;   //0x12, Type 0x9
            public int largetsVtxl;       //In 0xC33 objects, this is the largest possible entry. Smaller entries will fill and skip past the remainder.

            public int totalStripFaces;  //0x14, Type 0x9
            public int globalStripOffset; //Unused in classic. Always 0x100 in 0xC33 since it's directly after OBJC in this one.
            public int totalVTXLCount;   //0x15, Type 0x8
            public int vtxlStartOffset;

            public int unkStructCount;        //0x16, Type 0x8 //Same value as below in classic.
            public int vsetCount;       //0x24, Type 0x9
            public int vsetOffset;
            public int psetCount;       //0x25, Type 0x9

            public int psetOffset;
            public int meshCount;    //0x17, Type 0x9
            public int meshOffset;
            public int mateCount;    //0x18, Type 0x8

            public int mateOffset;
            public int rendCount;    //0x19, Type 0x8
            public int rendOffset;
            public int shadCount;    //0x1A, Type 0x8

            public int shadOffset;
            public int tstaCount;    //0x1B, Type 0x8
            public int tstaOffset;
            public int tsetCount;    //0x1C, Type 0x8

            public int tsetOffset;
            public int texfCount;    //0x1D, Type 0x8
            public int texfOffset;

            public BoundingVolume bounds; //0x1E, 0x1F, 0x20, 0x21
            public int unkCount0;

            public int unrmOffset; //Never set if unused. 

            //End of classic OBJC

            public int vtxeCount;
            public int vtxeOffset;
            public int bonePaletteOffset; //0xC33 only uses a single bone palette

            public int fBlock0; //These 4 seemingly do nothing in normal models, but have values in some trp/tro variations of the format.
            public int fBlock1; //Maybe these are all shorts?
            public int fBlock2;
            public int fBlock3;

            public int unkStruct1Count;
            public int unkStruct1Offset;
            public int pset2Count;
            public int pset2Offset;

            public int mesh2Count;
            public int mesh2Offset;
            public int globalStrip3Offset;
            public int globalStrip3LengthCount;

            public int globalStrip3LengthOffset;
            public int unkPointArray1Offset;
            public int unkPointArray2Offset;
            public int unkCount3;
        }

        public static OBJC ReadOBJC(BufferedStreamReader streamReader)
        {
            OBJC objc = new OBJC();

            objc.type = streamReader.Read<int>();
            objc.size = streamReader.Read<int>();
            objc.unkMeshValue = streamReader.Read<int>();
            objc.largetsVtxl = streamReader.Read<int>();

            objc.totalStripFaces = streamReader.Read<int>();
            objc.globalStripOffset = streamReader.Read<int>();
            objc.totalVTXLCount = streamReader.Read<int>();
            objc.vtxlStartOffset = streamReader.Read<int>();

            objc.unkStructCount = streamReader.Read<int>();
            objc.vsetCount = streamReader.Read<int>();
            objc.vsetOffset = streamReader.Read<int>();
            objc.psetCount = streamReader.Read<int>();

            objc.psetOffset = streamReader.Read<int>();
            objc.meshCount = streamReader.Read<int>();
            objc.meshOffset = streamReader.Read<int>();
            objc.mateCount = streamReader.Read<int>();

            objc.mateOffset = streamReader.Read<int>();
            objc.rendCount = streamReader.Read<int>();
            objc.rendOffset = streamReader.Read<int>();
            objc.shadCount = streamReader.Read<int>();

            objc.shadOffset = streamReader.Read<int>();
            objc.tstaCount = streamReader.Read<int>();
            objc.tstaOffset = streamReader.Read<int>();
            objc.tsetCount = streamReader.Read<int>();

            objc.tsetOffset = streamReader.Read<int>();
            objc.texfCount = streamReader.Read<int>();
            objc.texfOffset = streamReader.Read<int>();

            objc.bounds = streamReader.Read<BoundingVolume>();
            objc.unkCount0 = streamReader.Read<int>();
            objc.unrmOffset = streamReader.Read<int>();

            if (objc.type >= 0xC32) 
            {

                objc.vtxeCount = streamReader.Read<int>();
                objc.vtxeOffset = streamReader.Read<int>();
                objc.bonePaletteOffset = streamReader.Read<int>();

                objc.fBlock0 = streamReader.Read<int>();
                objc.fBlock1 = streamReader.Read<int>();
                objc.fBlock2 = streamReader.Read<int>();
                objc.fBlock3 = streamReader.Read<int>();

                objc.unkStruct1Count = streamReader.Read<int>();
                objc.unkStruct1Offset = streamReader.Read<int>();
                objc.pset2Count = streamReader.Read<int>();
                objc.pset2Offset = streamReader.Read<int>();

                if (objc.type >= 0xC33)
                {
                    objc.mesh2Count = streamReader.Read<int>();
                    objc.mesh2Offset = streamReader.Read<int>();
                    objc.globalStrip3Offset = streamReader.Read<int>();
                    objc.globalStrip3LengthCount = streamReader.Read<int>();

                    objc.globalStrip3LengthOffset = streamReader.Read<int>();
                    objc.unkPointArray1Offset = streamReader.Read<int>();
                    objc.unkPointArray2Offset = streamReader.Read<int>();
                    objc.unkCount3 = streamReader.Read<int>();
                }
                //objc.type = 0xC33; //0xC33 is essentially the same so we can treat it as that from here. Its objc just doesn't have those last 2 fields or the associated arrays.
            }

            return objc;
        }

        public struct MESH
        {
            public short flags; //0xB0, type 0x9, byte 0 and byte 1 //0x17 usually, 0x11 usually in 0xC33
            public short unkShort0; //0xB0, type 0x9, byte 2 and byte 3 //0, 0x9, sometimes 0x10
            public byte unkByte0; //0xC7, type 0x9, byte 0 //0x80 or very close. Unknown what it affects
            public byte unkByte1; //0xC7, type 0x9, byte 1 //0x64 or sometimes 0x63
            public short unkShort1; //0xC7, type 0x9, byte 2 and byte 3 //always 0?
            public int mateIndex;   //0xB1, type 0x8
            public int rendIndex;   //0xB2, type 0x8

            public int shadIndex;   //0xB3, type 0x8
            public int tsetIndex;   //0xB4, type 0x8
            public int baseMeshNodeId; //0xB5, type 0x8 //Used for assigning rigid weights in absence of vertex weights. 0 for basewear. 
                                       //Otherwise, takes the value of the dummy bone auto generated per mesh exported. 
                                       //Said bones are stored after regular bones, but before physics bones.
            public int vsetIndex; //0xC0, type 0x8  //Same as mesh index

            public int psetIndex; //0xC1, type 0x8 //Same as mesh index
            public int baseMeshDummyId; //0xC2, type 0x9  //Autogenerated mesh dummy bones have associated ids based on the order they were created. 
                                           //ie. basemesh0, basemesh1, etc. This stores that number or is 0 for basewear.
            public int unkInt0; //0xCD, 0x8 //Usually 0;
            public int reserve0; //0
        }

        public struct MATE
        {
            //Vector4 colors are assumedly based on the particular shader
            public Vector4 diffuseRGBA; //0x30, type 0x4A, variant 0x2 //alpha is always 1 in official
            public Vector4 unkRGBA0;    //0x31, type 0x4A, variant 0x2 //Defaults are .9 .9 .9 1.0
            public Vector4 _sRGBA;      //0x32, type 0x4A, variant 0x2 //Seemingly RGBA for the specular map. 
                                        //Value 3 affects self illum, just as blue, the third RGBA section, affects this in the _s map. A always observed as 1.
            public Vector4 unkRGBA1;    //0x33, type 0x4A, variant 0x2 //Works same as above? A always observed as 1. In NGS, this seems different.

            public int reserve0;        //0x34, type 0x9
            public float unkFloat0;     //0x35, type 0xA //Typically 8 or 32. I default it to 8. Possibly one of the 0-100 material values in max.
            public float unkFloat1;     //0x36, type 0xA //Tyipcally 1
            public int unkInt0;         //0x37, type 0x9 //Typically 100. Almost definitely a Max material 0-100 thing. (PSO2 models pass through 3ds Max in development at some point.)

            public int unkInt1;         //0x38, type 0x9 //Usually 0, sometimes other things
            public PSO2String alphaType; //0x3A, type 0x2 //Fixed length string for the alpha type of the mat. "opaque", "hollow", "blendalpha", and "add" are
                                         //all valid. Add is additive, and uses diffuse alpha for glow effects. Others may be used to denote collision types
            public PSO2String matName;   //0x39, type 0x2 

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(Object obj)
            {
                MATE c = (MATE)obj;
                // Optimization for a common success case.
                if (Object.ReferenceEquals(this, c))
                {
                    return true;
                }

                // If run-time types are not exactly the same, return false.
                if (this.GetType() != c.GetType())
                {
                    return false;
                }

                return (reserve0 == c.reserve0) && (unkFloat0 == c.unkFloat0) && (unkFloat1 == c.unkFloat1) && (unkInt0 == c.unkInt0) && (unkInt1 == c.unkInt1)
                    && (alphaType == c.alphaType) && (matName == c.matName) && isEqualVec4(diffuseRGBA, c.diffuseRGBA) && isEqualVec4(unkRGBA0, c.unkRGBA0)
                    && isEqualVec4(_sRGBA, c._sRGBA) && isEqualVec4(unkRGBA1, c.unkRGBA1);
            }

            public static bool operator ==(MATE lhs, MATE rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(MATE lhs, MATE rhs) => !(lhs == rhs);
        }

        public struct REND
        {
            public int tag { get; set; }      //0x40, type 0x9 //Always 0x1FF
            public int unk0 { get; set; }     //0x41, type 0x9 //3 usually
            public int twosided { get; set; }  //0x42, type 0x9 //0 for backface cull, 1 for twosided, 2 used in persona live dance models for unknown purposes (backface only?)
            public int int_0C { get; set; }  //0x43, type 0x9 //Maybe related to blend sort order? I'm not sure...

            //Next 12 values appear related, maybe to some texture setting? There are 3 sets that start with 5, first two go to 6, all go to 1, thhen 4th is typically different.
            public int sourceAlpha { get; set; }  //0x44, type 0x9 //5 usually
            public int destinationAlpha { get; set; }  //0x45, type 0x9 //6 usually
            public int unk3 { get; set; }  //0x46, type 0x9 //1 usually
            public int unk4 { get; set; }  //0x47, type 0x9 //0 usually

            public int unk5 { get; set; }  //0x48, type 0x9 //5 usually
            public int unk6 { get; set; }  //0x49, type 0x9 //6 usually
            public int unk7 { get; set; }  //0x4A, type 0x9 //1 usually. Another alpha setting, perhaps for multi/_s map?
            public int unk8 { get; set; }  //0x4B, type 0x9 //1 usually.

            public int unk9 { get; set; }   //0x4C, type 0x9 //5 usually
            public int alphaCutoff { get; set; }  //0x4D, type 0x9 //0-256, (Assumedly value of alpha at which a pixel is rendered invisible vs fully visible)
            public int unk11 { get; set; }  //0x4E, type 0x9 //1 usually
            public int unk12 { get; set; }  //0x4F, type 0x9 //4 usually

            public int unk13 { get; set; }  //0x50, type 0x9 //1 usually

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return Equals((REND)obj);
            }

            public bool Equals(REND c)
            {

                // Optimization for a common success case.
                if (Object.ReferenceEquals(this, c))
                {
                    return true;
                }

                // If run-time types are not exactly the same, return false.
                if (this.GetType() != c.GetType())
                {
                    return false;
                }

                return (tag == c.tag) && (unk0 == c.unk0) && (twosided == c.twosided) && (int_0C == c.int_0C) && (sourceAlpha == c.sourceAlpha) && (destinationAlpha == c.destinationAlpha) && (unk3 == c.unk3) 
                    && (unk4 == c.unk4) && (unk5 == c.unk5) && (unk6 == c.unk6) && (unk7 == c.unk7) && (unk8 == c.unk8) && (unk9 == c.unk9) && (alphaCutoff == c.alphaCutoff) && (unk11 == c.unk11) 
                    && (unk12 == c.unk12) && (unk13 == c.unk13);
            }

            public static bool operator ==(REND lhs, REND rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(REND lhs, REND rhs) => !(lhs == rhs);
        }

        //Contains information about the triangle strip sets
        public struct PSET
        {
            public int tag; //0xC6, type 0x9 //0x2100 in classic, 0x1000 in aqp 0xC33. May actually be two int16s since it's 0x140 for the second two bytes in .trp. May not mean much
            public int faceGroupCount; //0xBB, type 0x9 //Number of face groups. In classic, this seems to ALWAYS be 0x1. However in NGS, this seems to be used. These groups seem to link
                                       //faces to specific groups for some purpose. Notably, the weird Vector3 arrays linked in objc.

            public int faceCountOffset; //Offset for the beginning of the correlating face data structure. 
            public int psetFaceCount; //0xBC, type 0x9 //This is actually the same count as the one at the offset above. Perhaps one would be used for triangle count and one would be used for true face count with another faceType above?

            public int faceOffset; //This is an offset to the beginning of the strip data. Unknown purpose in 0xC33 variant
            public int stripStartCount; //0xC5, type 0x9 //Unused in classic. Provides starting id in global strip list for 0xC33.
        }

        //SHAder Pixel? Seemingly only in VTBF. One per shader set so unlikely to be a standin for shape.
        //Empty in all observed cases, just having a start struct tag and an end struct tag.
        public struct SHAP
        {
        }

        public class SHAD
        {
            public int unk0; //0x90, type 0x9 //Always 0?
            public PSO2String pixelShader; //0x91, type 0x2 //Pixel Shader string
            public PSO2String vertexShader; //0x92, type 0x2 //Vertex Shader string
            public int shadDetailOffset; //0x93, type 0x9 //Unused in classic. //Offset to struct containing details for the shadExtra area, including a count needed to read it.
            public int shadExtraOffset; //Unused in classic. Not read in some versions of NIFL Tool, causing misalignments. Doesn't exist in VTBF, so perhaps added later on.
                                        //Offset to struct containing extra shader info with areas for some int16s and floats.

            public virtual SHAD Clone()
            {
                SHAD newShad = new SHAD();
                newShad.unk0 = unk0;
                newShad.pixelShader = PSO2String.GeneratePSO2String(pixelShader.GetBytes());
                newShad.vertexShader = PSO2String.GeneratePSO2String(vertexShader.GetBytes());
                newShad.shadDetailOffset = shadDetailOffset;
                newShad.shadExtraOffset = shadExtraOffset;

                return newShad;
            }
        }

        public static SHAD ReadSHAD(BufferedStreamReader streamReader)
        {
            SHAD shad = new SHAD();
            shad.unk0 = streamReader.Read<int>();
            shad.pixelShader = streamReader.Read<PSO2String>();
            shad.vertexShader = streamReader.Read<PSO2String>();
            shad.shadDetailOffset = streamReader.Read<int>();
            shad.shadExtraOffset = streamReader.Read<int>();

            return shad;
        }

        //A texture set
        public class TSET
        {
            public int unkInt0; //0
            public int texCount; //0-?. Technically not using any textures is valid based on observation. 0-4 is 4 int32s. Otherwise, each is a byte for 16 bytes total with empties being 0xFF. 
            public int unkInt1;  //0
            public int unkInt2;  //0

            public int unkInt3;  //0

            public List<int> tstaTexIDs = new List<int>(); //Ids of textures in set based on their order in the file, starting at 0. -1/FF if no texture in slot

            public TSET()
            {
            }

            public TSET Clone()
            {
                TSET newTset = new TSET();
                newTset.unkInt0 = unkInt0;
                newTset.texCount = texCount;
                newTset.unkInt1 = unkInt1;
                newTset.unkInt2 = unkInt2;

                newTset.unkInt3 = unkInt3;

                newTset.tstaTexIDs = new List<int>(tstaTexIDs);

                return newTset;
            }
        }

        public static TSET ReadTSET(BufferedStreamReader streamReader)
        {
            TSET tset = new TSET();
            tset.unkInt0 = streamReader.Read<int>();
            tset.texCount = streamReader.Read<int>();
            tset.unkInt1 = streamReader.Read<int>();
            tset.unkInt2 = streamReader.Read<int>();

            tset.unkInt3 = streamReader.Read<int>();

            long structEnd = streamReader.Position() + 0x10;
            //This section will be the classic int based indexing if 4 or less textures (0 is a valid count). 0xFFFFFFFF signifies a null texture.
            //If this section has more textures than 4, ids will be listed in bytes with remainder filled in by 0xFF.
            if (tset.texCount > 4)
            {
                for (int i = 0; i < tset.texCount; i++)
                {
                    byte temp = streamReader.Read<byte>();
                    if (temp != 0xFF)
                    {
                        tset.tstaTexIDs.Add(temp);
                    }
                }
            } else
            {
                for(int i = 0; i < tset.texCount; i++)
                {
                    int temp = streamReader.Read<int>();
                    if(temp >= 0)
                    {
                        tset.tstaTexIDs.Add(temp);
                    }
                }
            }
            streamReader.Seek(structEnd, System.IO.SeekOrigin.Begin);

            return tset;
        }

        //Texture Settings
        public struct TSTA
        {
            public int tag; //0x60, type 0x9 //0x16, always in classic. In 0xC33, often 0x17
            public int texUsageOrder; //0x61, type 0x9  //0,1,2, 3 etc. PSO2 TSETs (Texture sets) require specific textures in specfic places. There should be a new TSTA if using a texture in a different slot for some reason.
            public int modelUVSet;    //0x62, type 0x9  //Observed as -1, 0, 1, and 2. 3 and maybe more theoretically usable. 0 is default, -1 is for _t maps or any map that doesn't use UVs. 1 is for _k maps.
            public Vector3 unkVector0; //0x63, type 0x4A, 0x1 //0, -0, 0, often.
            public float unkFloat2; //0x64, type 0x9 //0
            public float unkFloat3; //0x65, type 0x9 //0

            public float unkFloat4; //0x66, type 0x9 //0
            public int unkInt3; //0x67, type 0x9 //1 or sometimes 3
            public int unkInt4; //0x68, type 0x9 //1 or sometimes 3
            public int unkInt5; //0x69, type 0x9 //1

            public float unkFloat0; //0x6A, type 0xA //0
            public float unkFloat1; //0x6B, type 0xA //0
            public PSO2String texName; //0x6C, type 0x2 //Texture filename (includes extension)
            public override bool Equals(object obj)
            {
                return Equals((TSTA)obj);
            }

            public bool Equals(TSTA c)
            {

                // Optimization for a common success case.
                if (Object.ReferenceEquals(this, c))
                {
                    return true;
                }

                // If run-time types are not exactly the same, return false.
                if (this.GetType() != c.GetType())
                {
                    return false;
                }

                return (tag == c.tag) && (texUsageOrder == c.texUsageOrder) && (modelUVSet == c.modelUVSet) && (unkVector0 == c.unkVector0) && (unkFloat2 == c.unkFloat2) && (unkFloat3 == c.unkFloat3) && (unkFloat4 == c.unkFloat4)
                    && (unkInt3 == c.unkInt3) && (unkInt4 == c.unkInt4) && (unkInt5 == c.unkInt5) && (unkFloat0 == c.unkFloat0) && (unkFloat1 == c.unkFloat1) && (texName == c.texName);
            }

            public static bool operator ==(TSTA lhs, TSTA rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(TSTA lhs, TSTA rhs) => !(lhs == rhs);
        }

        //Laid out in same order as TSTA. Seemingly redundant.
        public struct TEXF
        {
            public PSO2String texName; //0x80, type 0x2 //Texture filename (includes extension)
        }


        //UNRM Struct - Seemingly links vertices split for various reasons(vertex colors per face, UVs, etc.).
        public class UNRM
        {
            public int vertGroupCountCount;  //0xDA, type 0x9 //Amount of vertex group counts (The amount of verts for each group of vertices in the mesh ids and vert ids).
            public int vertGroupCountOffset; //Offset for listing of vertex group counts. 
            public int vertCount; //0xDC, type 0x9 //Total vertices in the mesh id and vertId data
            public int meshIdOffset;
            public int vertIDOffset;
            public double padding0;
            public int padding1;
            public List<int> unrmVertGroups = new List<int>(); //0xDB, type 0x89
            //Align to 0x10
            public List<List<int>> unrmMeshIds = new List<List<int>>(); //0xDD, type 0x89
            //Align to 0x10
            public List<List<int>> unrmVertIds = new List<List<int>>(); //0xDE, type 0x89
            //Align to 0x10

            public UNRM Clone()
            {
                UNRM newUnrm = new UNRM();
                newUnrm.vertGroupCountCount = vertGroupCountCount;
                newUnrm.vertGroupCountOffset = vertGroupCountOffset;
                newUnrm.vertCount = vertCount;
                newUnrm.meshIdOffset = meshIdOffset;
                newUnrm.vertIDOffset = vertIDOffset;
                newUnrm.padding0 = padding0;
                newUnrm.padding1 = padding1;
                newUnrm.unrmVertGroups = new List<int>(unrmVertGroups);
                newUnrm.unrmMeshIds = unrmMeshIds.ConvertAll(id => new List<int>( (int[])id.ToArray().Clone() )).ToList();
                newUnrm.unrmVertIds = unrmVertIds.ConvertAll(id => new List<int>((int[])id.ToArray().Clone())).ToList();

                return newUnrm;
            }
        }
        public struct VSET
        {
            public int vertDataSize;   //0xB6, Type 0x9 //In 0xC33, should always be OBJC largest size due to VTXL structure changes.
            public int vtxeCount; //0xBF, Type 0x9 //Number of data struct types per vertex in classic. Index of VTXE struct in 0xC33
            public int vtxeOffset;      //Unused in 0xC33
            public int vtxlCount;      //0xB9, Type 0x9 //Number of VTXL structs/Vertices

            public int vtxlOffset;     //Unused in 0xC33
            public int vtxlStartVert;       //0xC4, Type 0x9 //Unused in classic. In 0xC33, specifies starting vertex for VSET within global VTXL list
            public int bonePaletteCount; //0xBD, Type 0x8 //Vertex groups can't have more than 15 bones. 
                                        //Unknown value in 0xC33. Replaces bone palette count and seems to be 0x??FFFFFF always. Perhaps a negative bonecount?
                                        //Still set to 0 if model does not use bones in 0xC33

            public int bonePaletteOffset;
            //In VTBF, VSET also contains bonePalette. 
            //0xBE. Entire entry omitted if count was 0. Type is 06 if single bone, 86 if multiple. Next is usually 0x8 or 0x6 (unknown what this really is), 
            //last is 0 based count as a byte.

            public int unk0;         //0xC8, Type 0x9 //Unknown
            public int unk1;         //0xCC, Type 0x9 //Unknown
            public int unk2;         //Likely an offset related to above as it's not present in VTBF.
                                     //Edge verts are what I christened the set of vertex ids seemingly split along where the mesh
                                     //had to be separated due to bone count limitations.
            public int edgeVertsCount;  //0xC9, Type 0x9

            public int edgeVertsOffset;
            //In VTBF, VSET also contains Edge Verts. 
            //0xCA. Entire entry omitted if count was 0. Type is 06 if single vert, 86 if multiple. Next is usually 0x8 or 0x6 (unknown what this really is), 
            //last is 0 based count as a byte.
        }

        //Definitions for data in VTXL array. Same as previous iteration in 0xC33, however relative addresses may be unused now, at least on the parsing side. 
        //VTXE is also separated from VTXL in 0xC33 as opposed to the pairing structure seen in previous variants.
        public class VTXE
        {
            public List<VTXEElement> vertDataTypes = new List<VTXEElement>();

            public VTXE Clone()
            {
                VTXE newVtxe = new VTXE();
                newVtxe.vertDataTypes = new List<VTXEElement>(vertDataTypes);

                return newVtxe;
            }
        }

        public struct VTXEElement
        {
            public int dataType;        //0xD0, type 0x9
            public int structVariation; //0xD1, type 0x9 //3 for Vector3, 4 for Vector4, 5 for 4 byte vert color, 2 for Vector2, 7 for 4 byte values
            public int relativeAddress; //0xD2, type 0x9
            public int reserve0;        //0xD3, type 0x9
        }

        //Vertex List. Used similarly in 0xC33, but handled different with different data types being put to use for various types.
        public class VTXL
        {
            public VTXL()
            {
            }

            public VTXL(int vertCount, VTXL modelVtxl)
            {
                vertPositions = new List<Vector3>(new Vector3[vertCount]); //Any vert should honestly have this if it's a proper vertex.
                if(modelVtxl.vertNormals.Count > 0)
                {
                    vertNormals = new List<Vector3>(new Vector3[vertCount]);
                }
                if (modelVtxl.vertNormalsNGS.Count > 0)
                {
                    vertNormalsNGS = new List<short[]>(new short[vertCount][]);
                }
                if (modelVtxl.vertBinormalList.Count > 0)
                {
                    vertBinormalList = new List<Vector3>(new Vector3[vertCount]);
                }
                if (modelVtxl.vertBinormalListNGS.Count > 0)
                {
                    vertBinormalListNGS = new List<short[]>(new short[vertCount][]);
                }
                if (modelVtxl.vertTangentList.Count > 0)
                {
                    vertTangentList = new List<Vector3>(new Vector3[vertCount]);
                }
                if (modelVtxl.vertTangentListNGS.Count > 0)
                {
                    vertTangentListNGS = new List<short[]>(new short[vertCount][]);
                }
                if (modelVtxl.vertColors.Count > 0)
                {
                    vertColors = new List<byte[]>(new byte[vertCount][]);
                }
                if (modelVtxl.vertColor2s.Count > 0)
                {
                    vertColor2s = new List<byte[]>(new byte[vertCount][]);
                }
                if (modelVtxl.uv1List.Count > 0)
                {
                    uv1List = new List<Vector2>(new Vector2[vertCount]);
                }
                if (modelVtxl.uv1ListNGS.Count > 0)
                {
                    uv1ListNGS = new List<short[]>(new short[vertCount][]);
                }
                if (modelVtxl.uv2ListNGS.Count > 0)
                {
                    uv2ListNGS = new List<short[]>(new short[vertCount][]);
                }
                if (modelVtxl.uv3ListNGS.Count > 0)
                {
                    uv3ListNGS = new List<short[]>(new short[vertCount][]);
                }
                if (modelVtxl.uv4ListNGS.Count > 0)
                {
                    uv4ListNGS = new List<short[]>(new short[vertCount][]);
                }
                if (modelVtxl.uv2List.Count > 0)
                {
                    uv2List = new List<Vector2>(new Vector2[vertCount]);
                }
                if (modelVtxl.uv3List.Count > 0)
                {
                    uv3List = new List<Vector2>(new Vector2[vertCount]);
                }
                if (modelVtxl.uv4List.Count > 0)
                {
                    uv4List = new List<Vector2>(new Vector2[vertCount]);
                }
                if (modelVtxl.vert0x22.Count > 0)
                {
                    vert0x22 = new List<short[]>(new short[vertCount][]);
                }
                if (modelVtxl.vert0x23.Count > 0)
                {
                    vert0x23 = new List<short[]>(new short[vertCount][]);
                }
                if (modelVtxl.vert0x24.Count > 0)
                {
                    vert0x24 = new List<short[]>(new short[vertCount][]);
                }
                if (modelVtxl.vert0x25.Count > 0)
                {
                    vert0x25 = new List<short[]>(new short[vertCount][]);
                }
                //These can... potentially be mutually exclusive, but the use cases for that are kind of limited and I don't and am not interested in handling them.
                if (modelVtxl.rawVertWeights.Count > 0)
                {
                    for (int i = 0; i < vertCount; i++)
                    {
                        rawVertWeights.Add(new List<float>());
                        rawVertWeightIds.Add(new List<int>());
                    }
                } else if(modelVtxl.vertWeights.Count > 0)
                {
                    vertWeights = new List<Vector4>(new Vector4[vertCount]);
                    vertWeightIndices = new List<byte[]>(new byte[vertCount][]);
                    for(int i = 0; i < vertCount; i++)
                    {
                        vertWeightIndices[i] = new byte[4];
                    }
                }
                if(modelVtxl.vertWeightsNGS.Count > 0)
                {
                    vertWeightsNGS = new List<ushort[]>(new ushort[vertCount][]);
                    for (int i = 0; i < vertCount; i++)
                    {
                        vertWeightsNGS[i] = new ushort[4];
                    }
                }

            }

            public List<Vector3> vertPositions = new List<Vector3>();
            public List<ushort[]> vertWeightsNGS = new List<ushort[]>(); //4 ushorts. Total should be FFFF between the 4. FFFF 0000 0000 0000 would be a full rigid weight.
            public List<short[]> vertNormalsNGS = new List<short[]>(); //4 16 bit values, value 4 being 0 always.
            public List<byte[]> vertColors = new List<byte[]>(); //4 bytes, BGRA
            public List<byte[]> vertColor2s = new List<byte[]>(); //4 bytes, BGRA?
            public List<byte[]> vertWeightIndices = new List<byte[]>(); //4 bytes
            public List<short[]> uv1ListNGS = new List<short[]>(); //2 16 bit values. UVs probably need to be vertically flipped for most software. I usually just import v as -v.
            public List<short[]> uv2ListNGS = new List<short[]>(); //Uncommon NGS uv2 variation
            public List<short[]> uv3ListNGS = new List<short[]>();
            public List<short[]> uv4ListNGS = new List<short[]>();
            public List<Vector2> uv2List = new List<Vector2>(); //For some reason 0xC33 seemingly retained vector2 data types for these, though only in some cases.
            public List<Vector2> uv3List = new List<Vector2>();
            public List<Vector2> uv4List = new List<Vector2>();
            public List<short[]> vert0x22 = new List<short[]>(); //This and the following type are 2 shorts seemingly that do... something. Only observed in 0xC33 player models at this time. 
            public List<short[]> vert0x23 = new List<short[]>(); //Possibly UV channels 5 and 6? Typically values are the same for every vertex in a mesh though.
            public List<short[]> vert0x24 = new List<short[]>();
            public List<short[]> vert0x25 = new List<short[]>();

            //Binormals and tangents for each face are calculated and each face's values for a particular vertex are summed and averaged for the result before being normalized
            //Though vertex position is used, due to the nature of the normalization applied during the process, resizing is unneeded.
            //For NGS, these are 4 shorts with value 4 always being 0.
            public List<short[]> vertTangentListNGS = new List<short[]>();
            public List<short[]> vertBinormalListNGS = new List<short[]>();

            //Old style lists
            public List<Vector4> vertWeights = new List<Vector4>();
            public List<Vector3> vertNormals = new List<Vector3>();
            public List<Vector2> uv1List = new List<Vector2>();
            public List<Vector3> vertTangentList = new List<Vector3>();
            public List<Vector3> vertBinormalList = new List<Vector3>();

            public List<ushort> bonePalette = new List<ushort>(); //Indices of particular bones are used for weight indices above
            public List<ushort> edgeVerts = new List<ushort>(); //No idea if this is used, but I fill it anyways

            //Helper variables
            //For raw data from a 3d editor
            public List<List<float>> rawVertWeights = new List<List<float>>();
            public List<List<int>> rawVertWeightIds = new List<List<int>>();

            //These are for help with splitting vertex data from face data. PSO2 does not allow storing data in faces so this is necessary to avoid problems.
            public List<int> rawVertId = new List<int>(); 
            public List<int> rawFaceId = new List<int>(); 

            //Holds processed weight info for accessing in external applications
            public List<Vector4> trueVertWeights = new List<Vector4>();
            public List<byte[]> trueVertWeightIndices = new List<byte[]>();

            public void convertFromLegacyTypes()
            {
                //Weights
                vertWeightsNGS.Clear();
                for (int i = 0; i < vertWeights.Count; i++)
                {
                    vertWeightsNGS.Add(new ushort[4]);
                    vertWeightsNGS[i][0] = ((ushort)(vertWeights[i].X * ushort.MaxValue));
                    vertWeightsNGS[i][1] = ((ushort)(vertWeights[i].Y * ushort.MaxValue));
                    vertWeightsNGS[i][2] = ((ushort)(vertWeights[i].Z * ushort.MaxValue));
                    vertWeightsNGS[i][3] = ((ushort)(vertWeights[i].W * ushort.MaxValue));
                }

                //Normals
                vertNormalsNGS.Clear();
                for (int i = 0; i < vertNormals.Count; i++)
                {
                    vertNormalsNGS.Add(new short[4]);
                    vertNormalsNGS[i][0] = ((short)(vertNormals[i].X * short.MaxValue)); if (vertNormalsNGS[i][0] == -short.MaxValue) { vertNormalsNGS[i][0]--; }
                    vertNormalsNGS[i][1] = ((short)(vertNormals[i].Y * short.MaxValue)); if (vertNormalsNGS[i][1] == -short.MaxValue) { vertNormalsNGS[i][1]--; }
                    vertNormalsNGS[i][2] = ((short)(vertNormals[i].Z * short.MaxValue)); if (vertNormalsNGS[i][2] == -short.MaxValue) { vertNormalsNGS[i][2]--; }
                    vertNormalsNGS[i][3] = (0);
                }

                //UV1List
                uv1ListNGS.Clear();
                for (int i = 0; i < uv1List.Count; i++)
                {
                    uv1ListNGS.Add(new short[2]);
                    uv1ListNGS[i][0] = ((short)(uv1List[i].X * short.MaxValue)); if (uv1ListNGS[i][0] == -short.MaxValue) { uv1ListNGS[i][0]--; }
                    uv1ListNGS[i][1] = ((short)(uv1List[i].Y * short.MaxValue)); if (uv1ListNGS[i][1] == -short.MaxValue) { uv1ListNGS[i][1]--; }
                }

                uv2ListNGS.Clear();
                for (int i = 0; i < uv2List.Count; i++)
                {
                    uv2ListNGS.Add(new short[2]);
                    uv2ListNGS[i][0] = ((short)(uv2List[i].X * short.MaxValue)); if (uv2ListNGS[i][0] == -short.MaxValue) { uv2ListNGS[i][0]--; }
                    uv2ListNGS[i][1] = ((short)(uv2List[i].Y * short.MaxValue)); if (uv2ListNGS[i][1] == -short.MaxValue) { uv2ListNGS[i][1]--; }
                }

                uv3ListNGS.Clear();
                for (int i = 0; i < uv3List.Count; i++)
                {
                    uv3ListNGS.Add(new short[2]);
                    uv3ListNGS[i][0] = ((short)(uv3List[i].X * short.MaxValue)); if (uv3ListNGS[i][0] == -short.MaxValue) { uv3ListNGS[i][0]--; }
                    uv3ListNGS[i][1] = ((short)(uv3List[i].Y * short.MaxValue)); if (uv3ListNGS[i][1] == -short.MaxValue) { uv3ListNGS[i][1]--; }
                }

                uv4ListNGS.Clear();
                for (int i = 0; i < uv4List.Count; i++)
                {
                    uv4ListNGS.Add(new short[2]);
                    uv4ListNGS[i][0] = ((short)(uv4List[i].X * short.MaxValue)); if (uv4ListNGS[i][0] == -short.MaxValue) { uv4ListNGS[i][0]--; }
                    uv4ListNGS[i][1] = ((short)(uv4List[i].Y * short.MaxValue)); if (uv4ListNGS[i][1] == -short.MaxValue) { uv4ListNGS[i][1]--; }
                }

                //Tangents
                vertTangentListNGS.Clear();
                for (int i = 0; i < vertTangentList.Count; i++)
                {
                    vertTangentListNGS.Add(new short[4]);
                    vertTangentListNGS[i][0] = ((short)(vertTangentList[i].X * short.MaxValue)); if (vertTangentListNGS[i][0] == -short.MaxValue) { vertTangentListNGS[i][0]--; }
                    vertTangentListNGS[i][1] = ((short)(vertTangentList[i].Y * short.MaxValue)); if (vertTangentListNGS[i][1] == -short.MaxValue) { vertTangentListNGS[i][1]--; }
                    vertTangentListNGS[i][2] = ((short)(vertTangentList[i].Z * short.MaxValue)); if (vertTangentListNGS[i][2] == -short.MaxValue) { vertTangentListNGS[i][2]--; }
                    vertTangentListNGS[i][3] = (0);
                }

                //Binormals
                vertBinormalListNGS.Clear();
                for (int i = 0; i < vertBinormalList.Count; i++)
                {
                    vertBinormalListNGS.Add(new short[4]);
                    vertBinormalListNGS[i][0] = ((short)(vertBinormalList[i].X * short.MaxValue)); if (vertBinormalListNGS[i][0] == -short.MaxValue) { vertBinormalListNGS[i][0]--; }
                    vertBinormalListNGS[i][0] = ((short)(vertBinormalList[i].Y * short.MaxValue)); if (vertBinormalListNGS[i][1] == -short.MaxValue) { vertBinormalListNGS[i][1]--; }
                    vertBinormalListNGS[i][0] = ((short)(vertBinormalList[i].Z * short.MaxValue)); if (vertBinormalListNGS[i][2] == -short.MaxValue) { vertBinormalListNGS[i][2]--; }
                    vertBinormalListNGS[i][0] = (0);
                }
            }

            public void convertToLegacyTypes(bool force = false)
            {
                if(force || vertWeights.Count == 0)
                {
                    //Weights
                    vertWeights.Clear();
                    for (int i = 0; i < vertWeightsNGS.Count; i++)
                    {
                        var weight = new Vector4();
                        weight.X = ((float)((float)vertWeightsNGS[i][0] / ushort.MaxValue));
                        weight.Y = ((float)((float)vertWeightsNGS[i][1] / ushort.MaxValue));
                        weight.Z = ((float)((float)vertWeightsNGS[i][2] / ushort.MaxValue));
                        weight.W = ((float)((float)vertWeightsNGS[i][3] / ushort.MaxValue));
                        weight = Vector4.Normalize(weight);
                        vertWeights.Add(weight);
                    }
                }

                if (force || vertNormals.Count == 0)
                {
                    //Normals
                    vertNormals.Clear();
                    for (int i = 0; i < vertNormalsNGS.Count; i++)
                    {
                        var normal = new Vector3();
                        normal.X = ((float)((float)vertNormalsNGS[i][0] / short.MaxValue));
                        normal.Y = ((float)((float)vertNormalsNGS[i][1] / short.MaxValue));
                        normal.Z = ((float)((float)vertNormalsNGS[i][2] / short.MaxValue));
                        normal = Vector3.Normalize(normal);
                        vertNormals.Add(normal);
                    }
                }

                if (force || uv1List.Count == 0)
                {
                    //UV1List
                    uv1List.Clear();
                    for (int i = 0; i < uv1ListNGS.Count; i++)
                    {
                        var uv = new Vector2();
                        uv.X = ((float)((float)uv1ListNGS[i][0] / short.MaxValue));
                        uv.Y = ((float)((float)uv1ListNGS[i][1] / short.MaxValue));
                        uv1List.Add(uv);
                    }
                }

                if (force || uv2List.Count == 0)
                {
                    //UV2List
                    uv2List.Clear();
                    for (int i = 0; i < uv2ListNGS.Count; i++)
                    {
                        var uv = new Vector2();
                        uv.X = ((float)((float)uv2ListNGS[i][0] / short.MaxValue));
                        uv.Y = ((float)((float)uv2ListNGS[i][1] / short.MaxValue));
                        uv2List.Add(uv);
                    }
                }

                if (force || uv3List.Count == 0)
                {
                    //UV2List
                    uv3List.Clear();
                    for (int i = 0; i < uv3ListNGS.Count; i++)
                    {
                        var uv = new Vector2();
                        uv.X = ((float)((float)uv3ListNGS[i][0] / short.MaxValue));
                        uv.Y = ((float)((float)uv3ListNGS[i][1] / short.MaxValue));
                        uv2List.Add(uv);
                    }
                }

                if (force || uv4List.Count == 0)
                {
                    //UV2List
                    uv4List.Clear();
                    for (int i = 0; i < uv4ListNGS.Count; i++)
                    {
                        var uv = new Vector2();
                        uv.X = ((float)((float)uv4ListNGS[i][0] / short.MaxValue));
                        uv.Y = ((float)((float)uv4ListNGS[i][1] / short.MaxValue));
                        uv2List.Add(uv);
                    }
                }

                if (force || vertTangentList.Count == 0)
                {
                    //Tangents
                    vertTangentList.Clear();
                    for (int i = 0; i < vertTangentListNGS.Count; i++)
                    {
                        var tangent = new Vector3();
                        tangent.X = ((float)((float)vertTangentListNGS[i][0] / short.MaxValue));
                        tangent.Y = ((float)((float)vertTangentListNGS[i][1] / short.MaxValue));
                        tangent.Z = ((float)((float)vertTangentListNGS[i][2] / short.MaxValue));
                        tangent = Vector3.Normalize(tangent);
                        vertTangentList.Add(tangent);
                    }
                }

                //Binormals
                if (force || vertBinormalList.Count == 0)
                {
                    vertBinormalList.Clear();
                    for (int i = 0; i < vertBinormalListNGS.Count; i++)
                    {
                        var binormal = new Vector3();
                        binormal.X = ((float)((float)vertBinormalListNGS[i][0] / short.MaxValue));
                        binormal.Y = ((float)((float)vertBinormalListNGS[i][1] / short.MaxValue));
                        binormal.Z = ((float)((float)vertBinormalListNGS[i][2] / short.MaxValue));
                        binormal = Vector3.Normalize(binormal);
                        vertBinormalList.Add(binormal);
                    }
                }
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

            //If last weight id is bone 0, move that to slot 0
            public void setLastId0sFirst()
            {
                if (vertWeights.Count > 0 && vertWeightIndices.Count > 0)
                {
                    //Account for bone palette 0 being ordered weird
                    for (int i = 0; i < vertWeights.Count; i++)
                    {
                        if (vertWeightIndices[i][1] == 0 && vertWeights[i].Y != 0 && vertWeightIndices[i][2] == 0)
                        {
                            vertWeights[i] = new Vector4(vertWeights[i].Y, vertWeights[i].X, vertWeights[i].Z, vertWeights[i].W);
                            vertWeightIndices[i] = new byte[] { vertWeightIndices[i][1], vertWeightIndices[i][0], vertWeightIndices[i][2], vertWeightIndices[i][3] };
                        }
                        if (vertWeightIndices[i][2] == 0 && vertWeights[i].Z != 0 && vertWeightIndices[i][3] == 0)
                        {
                            vertWeights[i] = new Vector4(vertWeights[i].Z, vertWeights[i].X, vertWeights[i].Y, vertWeights[i].W);
                            vertWeightIndices[i] = new byte[] { vertWeightIndices[i][2], vertWeightIndices[i][0], vertWeightIndices[i][1], vertWeightIndices[i][3] };
                        }
                        if (vertWeightIndices[i][3] == 0 && vertWeights[i].W != 0)
                        {
                            vertWeights[i] = new Vector4(vertWeights[i].W, vertWeights[i].X, vertWeights[i].Y, vertWeights[i].Z);
                            vertWeightIndices[i] = new byte[] { vertWeightIndices[i][3], vertWeightIndices[i][0], vertWeightIndices[i][1], vertWeightIndices[i][2] };
                        }
                    }
                }
            }

            //If id is 0 and not in slot 0, it goes to slot 0
            public void setId0sFirst()
            {
                if (vertWeights.Count > 0 && vertWeightIndices.Count > 0)
                {
                    //Account for bone palette 0 being ordered weird
                    for (int i = 0; i < vertWeights.Count; i++)
                    {
                        if (vertWeightIndices[i][1] == 0 && vertWeights[i].Y != 0)
                        {
                            vertWeights[i] = new Vector4(vertWeights[i].Y, vertWeights[i].X, vertWeights[i].Z, vertWeights[i].W);
                            vertWeightIndices[i] = new byte[] { vertWeightIndices[i][1], vertWeightIndices[i][0], vertWeightIndices[i][2], vertWeightIndices[i][3] };
                        }
                        if (vertWeightIndices[i][2] == 0 && vertWeights[i].Z != 0)
                        {
                            vertWeights[i] = new Vector4(vertWeights[i].Z, vertWeights[i].X, vertWeights[i].Y, vertWeights[i].W);
                            vertWeightIndices[i] = new byte[] { vertWeightIndices[i][2], vertWeightIndices[i][0], vertWeightIndices[i][1], vertWeightIndices[i][3] };
                        }
                        if (vertWeightIndices[i][3] == 0 && vertWeights[i].W != 0)
                        {
                            vertWeights[i] = new Vector4(vertWeights[i].W, vertWeights[i].X, vertWeights[i].Y, vertWeights[i].Z);
                            vertWeightIndices[i] = new byte[] { vertWeightIndices[i][3], vertWeightIndices[i][0], vertWeightIndices[i][1], vertWeightIndices[i][2] };
                        }
                    }
                }
            }

            public void AssureSumOfOneOnWeights()
            {
                if (vertWeights.Count > 0 && vertWeightIndices.Count > 0)
                {
                    for (int i = 0; i < vertWeights.Count; i++)
                    {
                        vertWeights[i] = SumWeightsTo1(vertWeights[i]);
                    }
                }
            }

            //PSO2 doesn't differentiate in the file how many weights a particular vert has. 
            //This allows one to condense the weight data
            public void createTrueVertWeights()
            {
                if (vertWeights.Count > 0 && vertWeightIndices.Count > 0)
                {
                    //Account for bone palette 0 being ordered weird
                    for (int i = 0; i < vertWeights.Count; i++)
                    {
                        Vector4 trueWeight = new Vector4();
                        List<byte> trueBytes = new List<byte>();
                        if (vertWeightIndices[i][0] != 0 || vertWeights[i].X != 0)
                        {
                            trueWeight = addById(trueWeight, vertWeights[i].X, trueBytes.Count);
                            trueBytes.Add(vertWeightIndices[i][0]);
                        }
                        if (vertWeightIndices[i][1] != 0 || vertWeights[i].Y != 0)
                        {
                            if(trueBytes.Contains(vertWeightIndices[i][1]))
                            {
                                trueWeight = addToId(trueWeight, vertWeights[i].Y, trueBytes.IndexOf(vertWeightIndices[i][1]));
                            } else
                            {
                                trueWeight = addById(trueWeight, vertWeights[i].Y, trueBytes.Count);
                                trueBytes.Add(vertWeightIndices[i][1]);
                            }
                        }
                        if (vertWeightIndices[i][2] != 0 || vertWeights[i].Z != 0)
                        {
                            if (trueBytes.Contains(vertWeightIndices[i][2]))
                            {
                                trueWeight = addToId(trueWeight, vertWeights[i].Z, trueBytes.IndexOf(vertWeightIndices[i][2]));
                            }
                            else
                            {
                                trueWeight = addById(trueWeight, vertWeights[i].Z, trueBytes.Count);
                                trueBytes.Add(vertWeightIndices[i][2]);
                            }
                        }
                        if (vertWeightIndices[i][3] != 0 || vertWeights[i].W != 0)
                        {
                            if (trueBytes.Contains(vertWeightIndices[i][3]))
                            {
                                trueWeight = addToId(trueWeight, vertWeights[i].W, trueBytes.IndexOf(vertWeightIndices[i][3]));
                            }
                            else
                            {
                                trueWeight = addById(trueWeight, vertWeights[i].W, trueBytes.Count);
                                trueBytes.Add(vertWeightIndices[i][3]);
                            }
                        }
                        //Ensure sum is as close as possible to 1.0.
                        trueWeight = SumWeightsTo1(trueWeight);

                        trueVertWeights.Add(trueWeight);
                        trueVertWeightIndices.Add(trueBytes.ToArray());
                    }
                }

            }

            //Fixes invalid bone assignments and weights them bone 0 instead.
            public void fixWeightsFromBoneCount(int maxBone = Int32.MaxValue)
            {
                if (vertWeights.Count > 0 && vertWeightIndices.Count > 0)
                {
                    for (int i = 0; i < vertWeights.Count; i++)
                    {
                        List<int> idList = new List<int>(); //List of ids to combine to bone0Id's value
                        int bone0Id = 0;
                        bool bone0Set = false;
                        for(int id = 0; id < vertWeightIndices[i].Length; id++)
                        {
                            int loopId = vertWeightIndices[i][id];
                            if(loopId == 0)
                            {
                                if(bone0Set == false)
                                {
                                    bone0Id = id;
                                    bone0Set = true;
                                    continue;
                                } else
                                {
                                    idList.Add(id);
                                }
                            } 

                            if(loopId > maxBone)
                            {
                                vertWeightIndices[i][id] = 0;
                                if (bone0Set == false)
                                {
                                    bone0Id = id;

                                } else
                                {
                                    idList.Add(id);
                                }
                            }
                        }

                        foreach(var id in idList)
                        {
                            vertWeights[i].Set(bone0Id,vertWeights[i].Get(id) + vertWeights[i].Get(bone0Id));
                            vertWeights[i].Set(id, 0);
                        }
                    }
                }
                if (rawVertWeights.Count > 0 && rawVertWeightIds.Count > 0)
                {
                    for (int i = 0; i < rawVertWeights.Count; i++)
                    {
                        List<int> idList = new List<int>(); //List of ids to combine to bone0Id's value
                        int bone0Id = 0;
                        bool bone0Set = false;
                        for (int id = 0; id < rawVertWeightIds[i].Count; id++)
                        {
                            int loopId = rawVertWeightIds[i][id];
                            if (loopId == 0)
                            {
                                if (bone0Set == false)
                                {
                                    bone0Id = id;
                                    bone0Set = true;
                                    continue;
                                }
                                else
                                {
                                    idList.Add(id);
                                }
                            }

                            if (loopId > maxBone)
                            {
                                rawVertWeightIds[i][id] = 0;
                                if (bone0Set == false)
                                {
                                    bone0Id = id;

                                }
                                else
                                {
                                    idList.Add(id);
                                }
                            }
                        }

                        foreach (var id in idList)
                        {
                            rawVertWeights[i][bone0Id] =  rawVertWeights[i][id] + rawVertWeights[i][bone0Id];
                            rawVertWeights[i][id] = 0;
                        }
                        if (idList.Count > 0)
                        {
                            for (int id = idList.Count - 1; id >= 0; id--)
                            {
                                rawVertWeightIds.RemoveAt(idList[id]);
                                rawVertWeights.RemoveAt(idList[id]);
                            }
                        }
                    }
                }
                if (trueVertWeights.Count > 0 && trueVertWeightIndices.Count > 0)
                {
                    for (int i = 0; i < trueVertWeights.Count; i++)
                    {
                        List<int> idList = new List<int>(); //List of ids to combine to bone0Id's value
                        int bone0Id = 0;
                        bool bone0Set = false;
                        for (int id = 0; id < trueVertWeightIndices[i].Length; id++)
                        {
                            int loopId = trueVertWeightIndices[i][id];
                            if (loopId == 0)
                            {
                                if (bone0Set == false)
                                {
                                    bone0Id = id;
                                    bone0Set = true;
                                    continue;
                                }
                                else
                                {
                                    idList.Add(id);
                                }
                            }

                            if (loopId > maxBone)
                            {
                                trueVertWeightIndices[i][id] = 0;
                                if (bone0Set == false)
                                {
                                    bone0Id = id;

                                }
                                else
                                {
                                    idList.Add(id);
                                }
                            }
                        }

                        foreach (var id in idList)
                        {
                            trueVertWeights[i].Set(bone0Id, trueVertWeights[i].Get(id) + trueVertWeights[i].Get(bone0Id));
                            trueVertWeights[i].Set(id, 0);
                        }
                        if(idList.Count > 0)
                        {
                            for (int id = idList.Count - 1; id >= 0; id--)
                            {
                                trueVertWeightIndices.RemoveAt(idList[id]);
                            }
                        }
                    }
                }

            }

            public static Vector4 SumWeightsTo1(Vector4 trueWeight)
            {
                double sum = trueWeight.X + trueWeight.Y + trueWeight.Z + trueWeight.W;
                trueWeight.X = (float)(trueWeight.X / sum);
                trueWeight.Y = (float)(trueWeight.Y / sum);
                trueWeight.Z = (float)(trueWeight.Z / sum);
                trueWeight.W = (float)(trueWeight.W / sum);
                return trueWeight;
            }

            public void processToPSO2Weights(bool useNGSWeights = false)
            {
                //Should be the same count for both lists, go through and populate as needed to cull weight counts that are too large
                for (int wt = 0; wt < rawVertWeights.Count; wt++)
                {
                    byte[] vertIds = new byte[4];
                    Vector4 vertWts = new Vector4();

                    //Descending sort to get 
                    for (int i = 0; i < rawVertWeights[wt].Count; i++)
                    {
                        for (int j = 0; j < rawVertWeights[wt].Count; j++)
                            if (rawVertWeights[wt][j] < rawVertWeights[wt][i])
                            {
                                var tmp0 = rawVertWeights[wt][i];
                                var tmp1 = rawVertWeightIds[wt][i];

                                rawVertWeights[wt][i] = rawVertWeights[wt][j];
                                rawVertWeightIds[wt][i] = rawVertWeightIds[wt][j];
                                rawVertWeights[wt][j] = tmp0;
                                rawVertWeightIds[wt][j] = tmp1;
                            }
                    }
                    switch(rawVertWeights[wt].Count)
                    {
                        //Case 0 really shouldn't happen
                        case 0:
                            vertWts.X = 1;
                            vertIds[0] = 0;
                            break;
                        case 1:
                            vertWts.X = rawVertWeights[wt][0];
                            vertIds[0] = (byte)rawVertWeightIds[wt][0];
                            break;
                        case 2:
                            vertWts.X = rawVertWeights[wt][0];
                            vertWts.Y = rawVertWeights[wt][1];
                            vertIds[0] = (byte)rawVertWeightIds[wt][0];
                            vertIds[1] = (byte)rawVertWeightIds[wt][1];
                            break;
                        case 3:
                            vertWts.X = rawVertWeights[wt][0];
                            vertWts.Y = rawVertWeights[wt][1];
                            vertWts.Z = rawVertWeights[wt][2];
                            vertIds[0] = (byte)rawVertWeightIds[wt][0];
                            vertIds[1] = (byte)rawVertWeightIds[wt][1];
                            vertIds[2] = (byte)rawVertWeightIds[wt][2];
                            break;
                        default:
                            vertWts.X = rawVertWeights[wt][0];
                            vertWts.Y = rawVertWeights[wt][1];
                            vertWts.Z = rawVertWeights[wt][2];
                            vertWts.W = rawVertWeights[wt][3];
                            vertIds[0] = (byte)rawVertWeightIds[wt][0];
                            vertIds[1] = (byte)rawVertWeightIds[wt][1];
                            vertIds[2] = (byte)rawVertWeightIds[wt][2];
                            vertIds[3] = (byte)rawVertWeightIds[wt][3];
                            break;
                    }
                    vertWts = SumWeightsTo1(vertWts);

                    vertWeightIndices.Add(vertIds);
                    vertWeights.Add(vertWts);
                    if(useNGSWeights)
                    {
                        ushort[] shortWeights = new ushort[] { (ushort)(vertWts.X * ushort.MaxValue), (ushort)(vertWts.Y * ushort.MaxValue), 
                            (ushort)(vertWts.Z * ushort.MaxValue), (ushort)(vertWts.W * ushort.MaxValue), };
                        vertWeightsNGS.Add(shortWeights);
                    }
                }
            }

            private Vector4 addById(Vector4 vec4, float value, int id)
            {
                switch (id)
                {
                    case 0:
                        vec4.X = value;
                        break;
                    case 1:
                        vec4.Y = value;
                        break;
                    case 2:
                        vec4.Z = value;
                        break;
                    case 3:
                        vec4.W = value;
                        break;
                }

                return vec4;
            }

            private Vector4 addToId(Vector4 vec4, float value, int id)
            {
                switch (id)
                {
                    case 0:
                        vec4.X += value;
                        break;
                    case 1:
                        vec4.Y += value;
                        break;
                    case 2:
                        vec4.Z += value;
                        break;
                    case 3:
                        vec4.W += value;
                        break;
                }

                return vec4;
            }

            public void AddRange(int vertCount, VTXL modelVtxl)
            {
                vertPositions.AddRange(new Vector3[vertCount]); //Any vert should honestly have this if it's a proper vertex.
                if (modelVtxl.vertNormals.Count > 0)
                {
                    vertNormals.AddRange(new Vector3[vertCount]);
                }
                if (modelVtxl.vertNormalsNGS.Count > 0)
                {
                    vertNormalsNGS.AddRange(new short[vertCount][]);
                }
                if (modelVtxl.vertBinormalList.Count > 0)
                {
                    vertBinormalList.AddRange(new Vector3[vertCount]);
                }
                if (modelVtxl.vertBinormalListNGS.Count > 0)
                {
                    vertBinormalListNGS.AddRange(new short[vertCount][]);
                }
                if (modelVtxl.vertTangentList.Count > 0)
                {
                    vertTangentList.AddRange(new Vector3[vertCount]);
                }
                if (modelVtxl.vertTangentListNGS.Count > 0)
                {
                    vertTangentListNGS.AddRange(new short[vertCount][]);
                }
                if (modelVtxl.vertColors.Count > 0)
                {
                    vertColors.AddRange(new byte[vertCount][]);
                }
                if (modelVtxl.vertColor2s.Count > 0)
                {
                    vertColor2s.AddRange(new byte[vertCount][]);
                }
                if (modelVtxl.uv1List.Count > 0)
                {
                    uv1List.AddRange(new Vector2[vertCount]);
                }
                if (modelVtxl.uv1ListNGS.Count > 0)
                {
                    uv1ListNGS.AddRange(new short[vertCount][]);
                }
                if (modelVtxl.uv2ListNGS.Count > 0)
                {
                    uv2ListNGS.AddRange(new short[vertCount][]);
                }
                if (modelVtxl.uv3ListNGS.Count > 0)
                {
                    uv3ListNGS.AddRange(new short[vertCount][]);
                }
                if (modelVtxl.uv4ListNGS.Count > 0)
                {
                    uv4ListNGS.AddRange(new short[vertCount][]);
                }
                if (modelVtxl.uv2List.Count > 0)
                {
                    uv2List.AddRange(new Vector2[vertCount]);
                }
                if (modelVtxl.uv3List.Count > 0)
                {
                    uv3List.AddRange(new Vector2[vertCount]);
                }
                if (modelVtxl.uv4List.Count > 0)
                {
                    uv4List.AddRange(new Vector2[vertCount]);
                }
                if (modelVtxl.vert0x22.Count > 0)
                {
                    vert0x22.AddRange(new short[vertCount][]);
                }
                if (modelVtxl.vert0x23.Count > 0)
                {
                    vert0x23.AddRange(new short[vertCount][]);
                }
                if (modelVtxl.vert0x24.Count > 0)
                {
                    vert0x24.AddRange(new short[vertCount][]);
                }
                if (modelVtxl.vert0x25.Count > 0)
                {
                    vert0x25.AddRange(new short[vertCount][]);
                }
                //These can... potentially be mutually exclusive, but the use cases for that are kind of limited and I don't and am not interested in handling them.
                if (modelVtxl.rawVertWeights.Count > 0)
                {
                    for (int i = rawVertWeights.Count; i < vertCount; i++)
                    {
                        rawVertWeights.Add(new List<float>());
                        rawVertWeightIds.Add(new List<int>());
                    }
                }
                else if (modelVtxl.vertWeights.Count > 0)
                {
                    int start = vertWeights.Count;
                    vertWeights.AddRange(new Vector4[vertCount]);
                    vertWeightIndices.AddRange(new byte[vertCount][]);
                    for (int i = start; i < start + vertCount; i++)
                    {
                        vertWeightIndices[i] = new byte[4];
                    }
                }
                if (modelVtxl.vertWeightsNGS.Count > 0)
                {
                    int start = vertWeightsNGS.Count;
                    vertWeightsNGS.AddRange(new ushort[vertCount][]);
                    for (int i = start; i < start + vertCount; i++)
                    {
                        vertWeightsNGS[i] = new ushort[4];
                    }
                }

            }

            public VTXL Clone()
            {
                VTXL newVTXL = new VTXL();

                newVTXL.vertPositions = new List<Vector3>(vertPositions);
                newVTXL.vertWeightsNGS = vertWeightsNGS.ConvertAll(wt => (ushort[])wt.Clone()).ToList();
                newVTXL.vertNormalsNGS = vertNormalsNGS.ConvertAll(nrm => (short[])nrm.Clone()).ToList();
                newVTXL.vertColors = vertColors.ConvertAll(clr => (byte[])clr.Clone()).ToList();
                newVTXL.vertColor2s = vertColor2s.ConvertAll(clr => (byte[])clr.Clone()).ToList();
                newVTXL.vertWeightIndices = vertWeightIndices.ConvertAll(wt => (byte[])wt.Clone()).ToList();
                newVTXL.uv1ListNGS = uv1ListNGS.ConvertAll(uv => (short[])uv.Clone()).ToList();
                newVTXL.uv2ListNGS = uv2ListNGS.ConvertAll(uv => (short[])uv.Clone()).ToList();
                newVTXL.uv3ListNGS = uv3ListNGS.ConvertAll(uv => (short[])uv.Clone()).ToList();
                newVTXL.uv4ListNGS = uv4ListNGS.ConvertAll(uv => (short[])uv.Clone()).ToList();
                newVTXL.uv1List = new List<Vector2>(uv1List);
                newVTXL.uv2List = new List<Vector2>(uv2List);
                newVTXL.uv3List = new List<Vector2>(uv3List);
                newVTXL.uv4List = new List<Vector2>(uv4List);
                newVTXL.vert0x22 = vert0x22.ConvertAll(uv => (short[])uv.Clone()).ToList();
                newVTXL.vert0x23 = vert0x23.ConvertAll(uv => (short[])uv.Clone()).ToList();
                newVTXL.vert0x24 = vert0x24.ConvertAll(uv => (short[])uv.Clone()).ToList();
                newVTXL.vert0x25 = vert0x25.ConvertAll(uv => (short[])uv.Clone()).ToList();
                newVTXL.vertTangentListNGS = vertTangentListNGS.ConvertAll(nrm => (short[])nrm.Clone()).ToList();
                newVTXL.vertBinormalListNGS = vertBinormalListNGS.ConvertAll(nrm => (short[])nrm.Clone()).ToList();
                newVTXL.vertWeights = new List<Vector4>(vertWeights);
                newVTXL.vertNormals = new List<Vector3>(vertNormals);
                newVTXL.vertTangentList = new List<Vector3>(vertTangentList);
                newVTXL.vertBinormalList = new List<Vector3>(vertBinormalList);
                newVTXL.bonePalette = new List<ushort>(bonePalette);
                newVTXL.edgeVerts = new List<ushort>(edgeVerts);
                newVTXL.rawVertWeights = rawVertWeights.ConvertAll(wt => new List<float>( (float[])wt.ToArray().Clone() )).ToList();
                newVTXL.rawVertWeightIds = rawVertWeightIds.ConvertAll(wt => new List<int>((int[])wt.ToArray().Clone())).ToList();
                newVTXL.rawVertId = new List<int>(rawVertId);
                newVTXL.rawFaceId = new List<int>(rawFaceId);
                newVTXL.trueVertWeights = new List<Vector4>(trueVertWeights);
                newVTXL.trueVertWeightIndices = trueVertWeightIndices.ConvertAll(wt => (byte[])wt.Clone()).ToList();

                return newVTXL;
            }
        }

        public void CreateTrueVertWeights()
        {
            foreach(var vtxl in vtxlList)
            {
                vtxl.createTrueVertWeights();
            }
        }

        public class stripData
        {
            public bool psoTris = false;
            public bool format0xC33 = false;
            public int triIdCount; //0xB7, type 0x9 
            public int reserve0;
            public int reserve1;
            public int reserve2;

            //The strip data is in a separate place in 0xC33, but will be placed here for convenience
            //Triangles should be interpreted as 0, 1, 2 followed by 0, 2, 1. While this results in degenerate faces, wireframe views ingame show they are rendered with these.
            public List<ushort> triStrips = new List<ushort>(); //0xB8, type 0x86 

            public List<Vector3> largeTriSet = new List<Vector3>(); //Shouldn't normally be used, mainly for special cases.

            public List<int> faceGroups = new List<int>(); //Count should match parent PSET's faceGroupCount. Unknown use. Seemingly groups sets of faces by order.

            public stripData()
            {
            }

            public stripData(ushort[] indices)
            {
                toStrips(indices);
            }

            public void toStrips(ushort[] indices)
            {
                List<ushort> stripList = new List<ushort>();
                NvStripifier stripifier = new NvStripifier();

                var nvStrips = stripifier.GenerateStripsReturner(indices, true);
                triIdCount = nvStrips[0].Indices.Length; //Should in theory be twice the actual length as it's counting bytes.
                triStrips = nvStrips[0].Indices.ToList();
            }

            public List<Vector3> GetTriangles(bool removeDegenFaces = true)
            {
                if(largeTriSet.Count > 0)
                {
                    return largeTriSet;
                }
                List<Vector3> tris = new List<Vector3>();
                if (format0xC33 == false)
                {
                    for (int vertIndex = 0; vertIndex < triStrips.Count - 2; vertIndex++)
                    {
                        //A degenerate triangle is a triangle with two references to the same vertex index. 
                        if (removeDegenFaces)
                        {
                            if (triStrips[vertIndex] == triStrips[vertIndex + 1] || triStrips[vertIndex] == triStrips[vertIndex + 2]
                                || triStrips[vertIndex + 1] == triStrips[vertIndex + 2])
                            {
                                continue;
                            }
                        }

                        //When index is odd, flip
                        if ((vertIndex & 1) > 0)
                        {
                            tris.Add(new Vector3(triStrips[vertIndex], triStrips[vertIndex + 2], triStrips[vertIndex + 1]));
                        }
                        else
                        {
                            if(psoTris == true)
                            {
                                tris.Add(new Vector3(triStrips[vertIndex + 1], triStrips[vertIndex + 2], triStrips[vertIndex]));
                            } else
                            {
                                tris.Add(new Vector3(triStrips[vertIndex], triStrips[vertIndex + 1], triStrips[vertIndex + 2]));
                            }
                        }
                    } 

                } else
                {
                    //0xC33 really just uses normal triangles. Yup.
                    for (int vertIndex = 0; vertIndex < triStrips.Count - 2; vertIndex += 3)
                    {
                        tris.Add(new Vector3(triStrips[vertIndex], triStrips[vertIndex + 1], triStrips[vertIndex + 2]));
                    }
                }

                return tris;
            }

            public stripData Clone()
            {
                stripData newStrip = new stripData();
                newStrip.psoTris = psoTris;
                newStrip.format0xC33 = format0xC33;
                newStrip.triIdCount = triIdCount;
                newStrip.reserve0 = reserve0;
                newStrip.reserve1 = reserve1;
                newStrip.reserve2 = reserve2;
                newStrip.triStrips = new List<ushort>(triStrips);
                newStrip.faceGroups = new List<int>(faceGroups);

                return newStrip;
            }
        }

        //Used for processing meshes for setting up compatibility with PSO2

        public class GenericTriangles
        {
            public List<VTXL> faceVerts = new List<VTXL>();
            public List<Vector3> triList = new List<Vector3>();
            public List<int> matIdList = new List<int>();
            public Dictionary<int, int> matIdDict = new Dictionary<int, int>(); //For helping convert mat ids if they need to be reindexed
            public List<uint> bonePalette = new List<uint>(); 
            public int baseMeshNodeId;
            public int baseMeshDummyId;
            public int vertCount;
            public string name;
            public GenericTriangles()
            {
            }

            public GenericTriangles(ushort[] tris, int[] matIds = null)
            {
                setUpVector3List(tris);
                matIdList = matIds.ToList();
            }

            public GenericTriangles(List<ushort> tris, List<int> matIds = null)
            {
                setUpVector3List(tris.ToArray());
                matIdList = matIds;
            }

            public GenericTriangles(List<Vector3> vec3s, List<int> matIds = null)
            {
                triList = vec3s;
                matIdList = matIds;
            }

            public List<ushort> toUshortList()
            {
                List<ushort> shorts = new List<ushort>();

                for (int i = 0; i < triList.Count; i++)
                {
                    shorts.Add((ushort)triList[i].X);
                    shorts.Add((ushort)triList[i].Y);
                    shorts.Add((ushort)triList[i].Z);
                }

                return shorts;
            }

            public ushort[] toUshortArray()
            {
                List<ushort> shorts = new List<ushort>();

                for (int i = 0; i < triList.Count; i++)
                {
                    shorts.Add((ushort)triList[i].X);
                    shorts.Add((ushort)triList[i].Y);
                    shorts.Add((ushort)triList[i].Z);
                }

                return shorts.ToArray();
            }

            public void setUpVector3List(ushort[] tris)
            {
                for (int i = 0; i < tris.Length; i += 3)
                {
                    triList.Add(new Vector3(tris[i], tris[i + 1], tris[i + 2]));
                }
            }

            public bool needsSplitting()
            {
                if (matIdList != null && matIdList.Count > 1)
                {
                    int firstId = matIdList[0];
                    for (int i = 1; i < matIdList.Count; i++)
                    {
                        if (firstId != matIdList[i])
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public GenericTriangles Clone()
            {
                GenericTriangles genTri = new GenericTriangles();
                genTri.faceVerts = faceVerts.ConvertAll(vtxl => vtxl.Clone()).ToList();
                genTri.triList = new List<Vector3>(triList);
                genTri.matIdList = new List<int>(matIdList);
                genTri.matIdDict = matIdDict.Keys.ToDictionary(x => x, x => matIdDict[x]);
                genTri.bonePalette = new List<uint>(bonePalette);
                genTri.baseMeshNodeId = baseMeshNodeId;
                genTri.baseMeshDummyId = baseMeshDummyId;
                genTri.vertCount = vertCount;
                genTri.name = name;

                return genTri;
            }
        }
        public class GenericMaterial
        {
            public List<string> texNames = null;
            public List<int> texUVSets = null;
            public List<string> shaderNames = null;
            public string blendType = null;
            public string specialType = null;
            public string matName = null;
            public int twoSided = 0; //0 False, 1 True. Higher values give unknown results

            public Vector4 diffuseRGBA = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            public Vector4 unkRGBA0 = new Vector4(.9f, .9f, .9f, 1.0f);
            public Vector4 _sRGBA = new Vector4(0f, 0f, 0f, 1.0f);
            public Vector4 unkRGBA1 = new Vector4(0f, 0f, 0f, 1.0f);

            public int reserve0 = 0;
            public float unkFloat0 = 8;
            public float unkFloat1 = 1;
            public int unkInt0 = 100;
            public int unkInt1 = 0;

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object o)
            {
                return Equals((GenericMaterial)o);
            }

            public bool Equals(GenericMaterial c)
            {

                // Optimization for a common success case.
                if (Object.ReferenceEquals(this, c))
                {
                    return true;
                }

                // If run-time types are not exactly the same, return false.
                if (this.GetType() != c.GetType())
                {
                    return false;
                }

                //texNames
                if (texNames == null || c.texNames == null)
                {
                    if (texNames != c.texNames)
                    {
                        return false;
                    }
                } else if (texNames.Count == c.texNames.Count)
                {
                    for (int i = 0; i < texNames.Count; i++)
                    {
                        if (texNames[i] != c.texNames[i])
                        {
                            return false;
                        }
                    }
                } else
                {
                    return false;
                }

                //texUvSets
                if (texUVSets == null || c.texUVSets == null)
                {
                    if (texUVSets != c.texUVSets)
                    {
                        return false;
                    }
                }
                else if (texUVSets.Count == c.texUVSets.Count)
                {
                    for (int i = 0; i < texUVSets.Count; i++)
                    {
                        if (texUVSets[i] != c.texUVSets[i])
                        {
                            return false;
                        }
                    }
                } else
                {
                    return false;
                }

                //shaderNames
                if (shaderNames == null || c.shaderNames == null)
                {
                    if (shaderNames != c.shaderNames)
                    {
                        return false;
                    }
                }
                else if(shaderNames.Count == c.shaderNames.Count)
                {
                    for (int i = 0; i < shaderNames.Count; i++)
                    {
                        if (shaderNames[i] != c.shaderNames[i])
                        {
                            return false;
                        }
                    }
                }else
                {
                    return false;
                }

                return (reserve0 == c.reserve0) && (unkFloat0 == c.unkFloat0) && (unkFloat1 == c.unkFloat1) && (unkInt0 == c.unkInt0) && (unkInt1 == c.unkInt1)
                    && (blendType == c.blendType) && isEqualVec4(diffuseRGBA, c.diffuseRGBA) && isEqualVec4(unkRGBA0, c.unkRGBA0)
                    && (specialType == c.specialType) && (twoSided == c.twoSided) && isEqualVec4(_sRGBA, c._sRGBA) && isEqualVec4(unkRGBA1, c.unkRGBA1);
            }

            public static bool operator ==(GenericMaterial lhs, GenericMaterial rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(GenericMaterial lhs, GenericMaterial rhs) => !(lhs == rhs);

            public GenericMaterial Clone()
            {
                GenericMaterial genMat = new GenericMaterial();
                genMat.texNames = new List<string>(texNames);
                genMat.texUVSets = new List<int>(texUVSets);
                genMat.shaderNames = new List<string>(shaderNames);
                genMat.blendType = blendType;
                genMat.specialType = specialType;
                genMat.matName = matName;
                genMat.twoSided = twoSided;

                genMat.diffuseRGBA = diffuseRGBA;
                genMat.unkRGBA0 = unkRGBA0;
                genMat._sRGBA = _sRGBA;
                genMat.unkRGBA1 = unkRGBA1;

                genMat.reserve0 = reserve0;
                genMat.unkFloat0 = unkFloat0;
                genMat.unkFloat1 = unkFloat1;
                genMat.unkInt0 = unkInt0;
                genMat.unkInt1 = unkInt1;

                return genMat;
            }
        }

        public void changeTexExtension(string ext)
        {
            for(int i = 0; i < texfList.Count; i++)
            {
                texfList[i].texName.SetString(Path.ChangeExtension(texfList[i].texName.GetString(), ext));
            }
            for (int i = 0; i < tstaList.Count; i++)
            {
                tstaList[i].texName.SetString(Path.ChangeExtension(tstaList[i].texName.GetString(), ext));
            }
        }

        //0xC33 variations of the format can recycle vtxl lists for multiple meshes. Neat, but not helpful for conversion purposes.
        public int splitVSETPerMesh()
        {
            bool continueSplitting = false;
            Dictionary<int, List<int>> vsetTracker = new Dictionary<int, List<int>>(); //Key int is a VSET, value is a list of indices for each mesh that uses said VSET
            for (int meshId = 0; meshId < meshList.Count; meshId++)
            {
                if (!vsetTracker.ContainsKey(meshList[meshId].vsetIndex))
                {
                    vsetTracker.Add(meshList[meshId].vsetIndex, new List<int>() { meshId });
                }
                else
                {
                    continueSplitting = true;
                    vsetTracker[meshList[meshId].vsetIndex].Add(meshId);
                }
            }

            if (continueSplitting)
            {
                VSET[] newVsetArray = new VSET[meshList.Count];
                VTXL[] newVtxlArray = new VTXL[meshList.Count];

                //Handle instances in which there are multiple of the same VSET used.
                //VTXL and VSETs should be cloned and updated as necessary while strips should be updated to match new vertex ids (strips using the same VTXL continue from old Ids, typically)
                foreach (var key in vsetTracker.Keys)
                {
                    if (vsetTracker[key].Count > 1)
                    {
                        foreach (int meshId in vsetTracker[key])
                        {
                            Dictionary<int, int> usedVerts = new Dictionary<int, int>();
                            VSET newVset = new VSET();
                            VTXL newVtxl = new VTXL();

                            int counter = 0;
                            for (int stripIndex = 0; stripIndex < strips[meshId].triStrips.Count; stripIndex++)
                            {
                                ushort id = strips[meshId].triStrips[stripIndex];
                                if (!usedVerts.ContainsKey(id))
                                {
                                    AquaObjectMethods.appendVertex(vtxlList[meshList[meshId].vsetIndex], newVtxl, id);
                                    usedVerts.Add(id, counter);
                                    counter++;
                                }
                                strips[meshId].triStrips[stripIndex] = (ushort)usedVerts[id];
                            }
                            var tempMesh = meshList[meshId];
                            tempMesh.vsetIndex = meshId;
                            meshList[meshId] = tempMesh;
                            newVsetArray[meshId] = newVset;
                            newVtxlArray[meshId] = newVtxl;
                        }
                    }
                    else
                    {
                        int meshId = vsetTracker[key][0];
                        newVsetArray[meshId] = vsetList[meshList[meshId].vsetIndex];
                        newVtxlArray[meshId] = vtxlList[meshList[meshId].vsetIndex];
                        var tempMesh = meshList[meshId];
                        tempMesh.vsetIndex = meshId;
                        meshList[meshId] = tempMesh;
                    }
                }
                vsetList = newVsetArray.ToList();
                vtxlList = newVtxlArray.ToList();
            }
            objc.vsetCount = vsetList.Count;

            return vsetList.Count;
        }

        public int getStripIndexCount()
        {
            int indexCount = 0;
            for (int i = 0; i < strips.Count; i++)
            {
                indexCount += strips[i].triIdCount;
            }
            return indexCount;
        }

        public int getVertexCount()
        {
            int vertCount = 0;
            for (int i = 0; i < vtxlList.Count; i++)
            {
                vertCount += vtxlList[i].vertPositions.Count;
            }
            return vertCount;
        }

        /// <summary>
        /// Gets a list of GenericMaterials representing all possible material component combinations used by this model based on its MESH structs.
        /// The idea would be condensing a list of traditional materials for external usage.
        /// A list of integers which map mesh indices to GenericMaterial ids is also output.
        /// </summary>
        /// <returns>List<GenericMaterial>, out List<int></returns>
        public List<GenericMaterial> GetUniqueMaterials(out List<int> meshMatMapping)
        {
            List<GenericMaterial> mats = new List<GenericMaterial>();
            meshMatMapping = new List<int>();

            for(int i = 0; i < meshList.Count; i++)
            {
                var curMesh = meshList[i];
                for(int msh = 0; msh < meshMatMapping.Count; msh++)
                {
                    var checkMesh = meshList[msh];
                    //Mate, rend, shad, and tset define what would make up a traditional material in a 3d program
                    if(curMesh.mateIndex == checkMesh.mateIndex && curMesh.rendIndex == checkMesh.rendIndex && curMesh.shadIndex == checkMesh.shadIndex && curMesh.tsetIndex == checkMesh.tsetIndex)
                    {
                        meshMatMapping.Add(meshMatMapping[msh]);
                        break;
                    }
                }

                if(meshMatMapping.Count - 1 != i)
                {
                    var curMate = mateList[curMesh.mateIndex];
                    var curRend = rendList[curMesh.rendIndex];
                    var shadNames = AquaObjectMethods.GetShaderNames(this, curMesh.shadIndex);
                    var texNames = AquaObjectMethods.GetTexListNames(this, curMesh.tsetIndex);
                    var texUvSets = AquaObjectMethods.GetTexListUVChannels(this, curMesh.tsetIndex);
                    GenericMaterial mat = new GenericMaterial();
                    mat.texNames = texNames;
                    mat.texUVSets = texUvSets;
                    mat.shaderNames = shadNames;
                    mat.blendType = curMate.alphaType.GetString();
                    mat.specialType = AquaObjectMethods.GetSpecialMatType(texNames);
                    mat.matName = curMate.matName.GetString();
                    mat.twoSided = curRend.twosided;

                    mat.diffuseRGBA = curMate.diffuseRGBA;
                    mat.unkRGBA0 = curMate.unkRGBA0;
                    mat._sRGBA = curMate._sRGBA;
                    mat.unkRGBA1 = curMate.unkRGBA1;

                    mat.reserve0 = curMate.reserve0;
                    mat.unkFloat0 = curMate.unkFloat0;
                    mat.unkFloat1 = curMate.unkFloat1;
                    mat.unkInt0 = curMate.unkInt0;
                    mat.unkInt1 = curMate.unkInt1;

                    mats.Add(mat);
                    meshMatMapping.Add(mats.Count - 1);
                }
            }

            return mats;
        }

        public void fixWeightsFromBoneCount(int maxBone = Int32.MaxValue)
        {
            for (int i = 0; i < vtxlList.Count; i++)
            {
                vtxlList[i].fixWeightsFromBoneCount(maxBone);
            }
        }
        public void ConvertToLegacyTypes()
        {
            for (int i = 0; i < vtxlList.Count; i++)
            {
                vtxlList[i].convertToLegacyTypes();
            }
        }

        public void addUnfilledUVs()
        {
            for(int i = 0; i < vtxlList.Count; i++)
            {
                var vtxl = vtxlList[i];
                bool addUV1 = !(vtxl.uv1List.Count > 0);
                bool addUV2 = !(vtxl.uv2List.Count > 0);
                bool addUV3 = !(vtxl.uv3List.Count > 0);
                bool addUV4 = !(vtxl.uv4List.Count > 0);
                bool addUV5 = !(vtxl.vert0x22.Count > 0);
                bool addUV6 = !(vtxl.vert0x23.Count > 0);
                bool addUV7 = !(vtxl.vert0x24.Count > 0);
                bool addUV8 = !(vtxl.vert0x25.Count > 0);

                for(int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    if(addUV1)
                    {
                        vtxl.uv1List.Add(new Vector2());
                    }
                    if (addUV2)
                    {
                        vtxl.uv2List.Add(new Vector2());
                    }
                    if (addUV3)
                    {
                        vtxl.uv3List.Add(new Vector2());
                    }
                    if (addUV4)
                    {
                        vtxl.uv4List.Add(new Vector2());
                    }
                    if (addUV5)
                    {
                        vtxl.vert0x22.Add(new short[2]);
                    }
                    if (addUV6)
                    {
                        vtxl.vert0x23.Add(new short[2]);
                    }
                    if (addUV7)
                    {
                        vtxl.vert0x24.Add(new short[2]);
                    }
                    if (addUV8)
                    {
                        vtxl.vert0x25.Add(new short[2]);
                    }
                }
            }
        }

        public void FixHollowMatNaming()
        {
            //Set wrongly assigned blendalphas to hollow for reexport purposes
            for (int i = 0; i < meshList.Count; i++)
            {
                var mesh = meshList[i];
                var mate = mateList[mesh.mateIndex];
                var rend = rendList[mesh.rendIndex];
                if ((mate.alphaType.GetString() == "opaque" || mate.alphaType.GetString() == "blendalpha") && rend.int_0C == 0 && rend.unk8 == 1 && rend.twosided == 2)
                {
                    //Account for recycled material, but separate rend
                    for (int j = 0; j < meshList.Count; j++)
                    {
                        var mesh2 = meshList[i];
                        if (mesh.mateIndex == mesh2.mateIndex && mesh.rendIndex != mesh2.rendIndex)
                        {
                            mesh.mateIndex = mateList.Count;
                            mateList.Add(mate);
                            mate = mateList[mateList.Count - 1];
                            objc.mateCount += 1;
                            meshList[i] = mesh;
                            break;
                        }
                    }

                    mate.alphaType.SetString("hollow");
                    mateList[mesh.mateIndex] = mate;
                }
            }
        }

        public abstract AquaObject Clone();

    }
}
