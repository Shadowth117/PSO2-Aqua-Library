using AquaModelLibrary.AquaStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AquaModelLibrary
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe class AquaObject
    {
        public AquaPackage.AFPBase afp;
        public AquaCommon.NIFL nifl;
        public AquaCommon.REL0 rel0;
        public OBJC objc;
        public List<VSET> vsetList = new List<VSET>();
        public List<VTXE> vtxeList = new List<VTXE>();
        public List<VTXL> vtxlList = new List<VTXL>();
        public List<PSET> psetList = new List<PSET>();
        public List<stripData> strips = new List<stripData>();
        public List<MESH> meshList = new List<MESH>();
        public List<MATE> mateList = new List<MATE>();
        public List<REND> rendList = new List<REND>();
        public List<SHAD> shadList = new List<SHAD>();
        public List<TSET> tsetList = new List<TSET>();
        public List<TSTA> tstaList = new List<TSTA>();
        public List<TEXF> texfList = new List<TEXF>();
        public UNRM unrms;
        public AquaCommon.NOF0 nof0;
        public AquaCommon.NEND nend;

        public enum VertFlags : int
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
            VertTangent = 0x20,
            VertBinormal = 0x21
        }

        public static Dictionary<int, int> VertDataTypes = new Dictionary<int, int>()
        {
            { (int)VertFlags.VertPosition, 0x3 }, //(0x0 Vertex Position) 
            { (int)VertFlags.VertWeight, 0x4 }, //(0x1 Vertex Weights)
            { (int)VertFlags.VertNormal, 0x3 }, //(0x2 Vertex Normal)
            { (int)VertFlags.VertColor, 0x5 }, //(0x3 Vertex Color)
            { (int)VertFlags.VertColor2, 0x5 }, //(0x4 Vertex Color 2?)
            { (int)VertFlags.VertWeightIndex, 0x7 }, //(0xb Weight Index)
            { (int)VertFlags.VertUV1, 0x2 }, //(0x10 UV1 Buffer)
            { (int)VertFlags.VertUV2, 0x2 }, //(0x11 UV2 Buffer)
            { (int)VertFlags.VertUV3, 0x2 }, //(0x12 UV3 Buffer)
            { (int)VertFlags.VertTangent, 0x3 }, //(0x20 Tangents)
            { (int)VertFlags.VertBinormal, 0x3 } //(0x21 Binormals)
        };

        public struct BoundingVolume
        {
            public Vector3 modelCenter; //0x1E, Type 0x4A, Count 0x1
            public float reserve0; 
            public float boundingRadius; //0x1F, Type 0xA                    //Distance of furthest point from the origin
            public Vector3 modelCenter2; //0x20, Type 0x4A, Count 0x1        //Model Center... again 
            public float reserve1;
            public Vector3 maxMinXYZDifference; //0x21, Type 0x4A, Count 0x1 //Distance between max/min of x, y and z divided by 2
            public float reserve2;
        }

        public struct OBJC
        {
            public int type;           //0x10, Type 0x8
            public int size;           //0x11, Type 0x8
            public int unkMeshValue;   //0x12, Type 0x9
            public int largetsVTXL;    //0x13, Type 0x8
            public int totalStripFaces;//0x14, Type 0x9
            public int reserve1;
            public int totalVTXLCount;//0x15, Type 0x8
            public int reserve2;
            public int unkMeshCount; //0x16, Type 0x8 //Same value as below
            public int vsetCount;    //0x24, Type 0x9
            public int vsetOffset;
            public int psetCount;    //0x25, Type 0x9
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
            public int unrmOffset; //Never set if unused
            public long padding0;
            public int padding1;
        }

        public struct MESH
        {
            public short tag; //0x17
            public short unkShort0; //0 or sometimes 10
            public byte unkByte0; //0x80
            public byte unkByte1; //0x64 or sometimes 0x63
            public short unkShort1; //always 0?
            public int mateIndex;
            public int rendIndex;
            public int shadIndex;
            public int tsetIndex;
            public int baseMeshNodeId; //Used for assigning rigid weights in absence of vertex weights. 0 for basewear. Otherwise, takes the value of the dummy bone auto generated per mesh exported. 
                                       //Said bones are stored after regular bones, but before physics bones.
            public int vsetIndex; //Same as mesh index
            public int psetIndex; //Same as mesh index
            public int baseMeshSequenceId; //Autogenerated mesh dummy bones have associated ids based on the order they were created. ie. basemesh0, basemesh1, etc. This stores that number or is 0 for basewear.
            public int unkInt0; //Usually 0;
            public int reserve0; //0
        }

        public struct MATE
        {
            public Vector4 diffuseRGBA; //alpha is always 1 in official
            public Vector4 unkRGBA0; //Defaults are .9 .9 .9 1.0
            public Vector4 _sRGBA; //Seemingly RGBA for the specular map. Value 3 affects self illum, just as blue, the third RGBA section, affects this in the _s map. A always observed as 1.
            public Vector4 unkRGBA1; //Works same as above? A always observed as 1.
            public int reserve0;
            public float unkFloat0; //Typically 8 or 32. I default it to 8. Possibly one of the 0-100 material values in max.
            public float unkFloat1; //Tyipcally 1
            public int unkInt0; //Typically 100. Almost definitely a Max material 0-100 thing.
            public int unkInt1; //Usually 0, sometimes other things
            public fixed byte alphaType[0x20]; //Fixed length string for the alpha type of the mat. "opaque", "hollow", "blendalpha", and "add" are all valid. Add is additive, and uses diffuse alpha for glow effects.
            public fixed byte matName[0x20];
        }

        public struct REND
        {
            public int tag; //Always 0x1FF
            public int unk0; //3 usually
            public int twosided; //0 for backface cull, 1 for twosided, 2 used in persona live dance models for unknown purposes (backface only?)
            public int notOpaque; //0 for opaque, 1 for anything else

            //Next 12 values appear related, maybe to some texture setting? There are 3 sets that start with 5, first two go to 6, all go to 1, thhen 4th is typically different.
            public int unk1; //5 usually
            public int unk2; //6 usually
            public int unk3; //1 usually
            public int unk4; //0 usually

            public int unk5; //5 usually
            public int unk6; //6 usually
            public int unk7; //1 usually. Another alpha setting, perhaps for multi/_s map?
            public int unk8; //1 usually.

            public int unk9; //5 usually
            public int unk10; //0-256, opaque alpha setting? (Assumedly value of alpha at which a pixel is rendered invisible vs fully visible)
            public int unk11; //1 usually
            public int unk12; //4 usually

            public int unk13; //1 usually
        }

        public struct SHAD
        {
            public int unk0; //Always 0?
            public fixed byte pixelShader[0x20]; //Pixel Shader string
            public fixed byte vertexShader[0x20]; //Vertex Shader string
            public int unk1; //Always 0?
            public int unk2; //Always 0? Not read in some versions of NIFL Tool, causing misalignments
        }

        //Texture Settings
        public struct TSTA
        {
            public int tag; //0x16, always
            public int texUsageOrder; //0,1,2, or 3. PSO2 TSETs (Texture sets) require specific textures in specfic places. There should be a new TSTA if using a texture in a different slot for some reason.
            public int modelUVSet; //Observed as -1, 1, and 2. 3 and maybe more theoretically usable. 1 is default, -1 is for _t maps or any map that doesn't use UVs. 2 is for _k maps.
            public Vector3 unkVector0; //0, -0, 0, often.
            public Vector3 unkVector1; //All 0s often
            public int unkInt0; //1 or sometimes 3
            public int unkInt1; //1 or sometimes 3
            public int unkInt2; //1
            public int unkInt3; //0
            public int unkInt4; //0
            public fixed byte texName[0x20]; //Texture filename (includes extension)
        }

        public struct TSET
        {
            public int unkInt0; //0
            public int texCount; //0-4. Technically not using any textures is valid.
            public int unkInt1; //0
            public int unkInt2; //0
            public int unkInt3; //0
            public fixed int tstaTexIDs[4]; //Ids of textures in set based on their order in the file, starting at 0. Write as -1 if no texture in slot (Not all shaders require all 4 slots to use a real texture)
        }

        //Laid out in same order as TSTA. Seemingly redundant.
        public struct TEXF
        {
            public fixed byte texName[0x20]; //Texture filename (includes extension)
        }

        //UNRM Struct - Seemingly links vertices split for various reasons(vertex colors per face, UVs, etc.).
        public class UNRM
        {
            public int vertGroupCountCount;  //Amount of vertex group counts (The amount of verts for each group of vertices in the mesh ids and vert ids).
            public int vertGroupCountOffset; //Offset for listing of vertex group counts. 
            public int vertCount;  //Total vertices in the mesh id and vertId data
            public int meshIdOffset;
            public int vertIDOffset;
            public double padding0;
            public int padding1;
            public List<int> unrmVertGroups = new List<int>();
            //Align to 0x10
            public List<List<int>> unrmMeshIds = new List<List<int>>();
            //Align to 0x10
            public List<List<int>> unrmVertIds = new List<List<int>>();
            //Align to 0x10
        }

        public struct VSET
        {
            public int vertDataSize;   //0xB6, Type 0x9
            public int vertTypesCount; //0xBF, Type 0x9 //Number of data struct types per vertex
            public int vtxeOffset;
            public int vtxlCount;      //0xB9, Type 0x9 //Number of VTXL structs/Vertices
            public int vtxlOffset;
            public int reserve0;       //0xC4, Type 0x9
            public int bonePaletteCount; //0xBD, Type 0x8 //Vertex groups can't have more than 15 bones
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
                //0xCA. Entire entry omitted if count was 0. Type is 06 if single bone, 86 if multiple. Next is usually 0x8 or 0x6 (unknown what this really is), 
                //last is 0 based count as a byte.
        }

        public class VTXE
        {
            public List<VTXEElement> vertDataTypes = new List<VTXEElement>();
        }

        public struct VTXEElement
        {
            public int dataType;        //D0, type 0x9
            public int structVariation; //D1, type 0x9 //3 for Vector3, 4 for Vector4, 5 for 4 byte vert color, 2 for Vector2, 7 for 4 byte values
            public int relativeAddress; //D2, type 0x9
            public int reserve0;        //D3, type 0x9
        }

        //Vertex List
        public class VTXL
        {
            public List<Vector3> vertPositions = new List<Vector3>();
            public List<Vector4> vertWeights = new List<Vector4>();
            public List<Vector3> vertNormals = new List<Vector3>();
            public List<byte[]> vertColors = new List<byte[]>(); //4 bytes, BGRA
            public List<byte[]> vertColor2s = new List<byte[]>(); //4 bytes, BGRA?
            public List<byte[]> vertWeightIndices = new List<byte[]>(); //4 bytes
            public List<Vector2> uv1List = new List<Vector2>(); //UVs probably need to be vertically flipped for most software. I usually just import v as -v.
            public List<Vector2> uv2List = new List<Vector2>();
            public List<Vector2> uv3List = new List<Vector2>();

            //Binormals and tangents for each face are calculated and each face's values for a particular vertex are summed and averaged for the result before being normalized
            //Though vertex position is used, due to the nature of the normalization applied during the process, resizing is unneeded.
            public List<Vector3> vertTangentList = new List<Vector3>();
            public List<Vector3> vertBinormalList =  new List<Vector3>();

            public List<short> bonePalette = new List<short>(); //Indices of particular bones are used for weight indices above
            public List<short> edgeVerts = new List<short>(); //No idea if this is used, but I fill it anyways

            public List<Vector4> trueVertWeights = new List<Vector4>();
            public List<byte[]> trueVertWeightIndices = new List<byte[]>();

            public List<Vector2> getUVFlipped(List<Vector2> uvList)
            {
                List<Vector2> uvs = uvList.ToList();

                for(int i = 0; i < uvs.Count; i++)
                {
                    Vector2 uv = uvs[i];
                    uv.Y = -uv.Y;
                    uvs[i] = uv;
                }

                return uvs;
            }

            //PSO2 doesn't differentiate in the file how many weights a particular vert has. 
            //This allows one to get the weight amount 
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
                            trueWeight = addById(trueWeight, vertWeights[i].Y, trueBytes.Count);
                            trueBytes.Add(vertWeightIndices[i][1]);
                        }
                        if (vertWeightIndices[i][2] != 0 || vertWeights[i].Z != 0)
                        {
                            trueWeight = addById(trueWeight, vertWeights[i].Z, trueBytes.Count);
                            trueBytes.Add(vertWeightIndices[i][2]);
                        }
                        if (vertWeightIndices[i][3] != 0 || vertWeights[i].W != 0)
                        {
                            trueWeight = addById(trueWeight, vertWeights[i].W, trueBytes.Count);
                            trueBytes.Add(vertWeightIndices[i][3]);
                        }

                        trueVertWeights.Add(trueWeight);
                        trueVertWeightIndices.Add(trueBytes.ToArray());
                    }
                }

            }

            private Vector4 addById(Vector4 vec4, float value, int id)
            {
                switch(id)
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
        }

        //Contains information about the traingle strip sets
        public struct PSET
        {
            public int tag; //0x2100
            public int faceType; //Assumedly facetype. Aqua models may support more standard polygons, but it's unclear whether that's a true feature or not. In any case, this value is 1 in all observed cases
            public int faceCountOffset; //Offset for the beginning of the correlating face data structure. 
            public int psetFaceCount; //This is actually the same count as the one at the offset above. Perhaps one would be used for triangle count and one would be used for true face count with another faceType above?
            public int faceOffset; //This is an offset to the beginning of the strip data.
            public int reserve0;
        }

        public class stripData
        {
            public int triCount;
            public int reserve0;
            public int reserve1;
            public int reserve2;

            //Triangles should be interpreted as 0, 1, 2 followed by 0, 2, 1. While this results in degenerate faces, wireframe views ingame show they are rendered with these.
            public List<short> triStrips = new List<short>();

            public List<Vector3> getTriangles(bool removeDegenFaces)
            {
                List<Vector3> tris = new List<Vector3>();

                for(int vertIndex = 0;  vertIndex < triStrips.Count - 2; vertIndex++)
                {
                    //A degenerate triangle is a triangle with two references to the same vertex index. 
                    if (removeDegenFaces)
                    {
                        if (triStrips[vertIndex] == triStrips[vertIndex + 1] || triStrips[vertIndex] == triStrips[vertIndex+2] 
                            || triStrips[vertIndex+1] == triStrips[vertIndex+2])
                        {
                            continue;
                        }
                    }

                    //When index is odd, flip
                    if((vertIndex & 1) > 0)
                    {
                        tris.Add(new Vector3(triStrips[vertIndex], triStrips[vertIndex + 2], triStrips[vertIndex + 1]));
                    } else
                    {
                        tris.Add(new Vector3(triStrips[vertIndex], triStrips[vertIndex + 1], triStrips[vertIndex + 2]));
                    }

                }

                return tris;
            }
        }
    }
}
