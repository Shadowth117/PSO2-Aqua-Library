using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Data.PSO2.MiscPSO2Structs;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //Props to DeadlyFugu for much of this class
    public unsafe class TCBTerrainConvex : AquaCommon
    {
        public List<Vector3> vertices = new();
        public List<TCBFace> faces = new();
        public List<TCBMaterial> materials = new();
        public NexusMesh nxsMesh = null;
        public TCB tcbInfo;

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            var type = sr.Peek<int>();

            //Proceed based on file variant
            if (type.Equals(0x626374))
            {
                tcbInfo = sr.Read<TCB>();

                //Read main TCB verts
                sr.Seek(tcbInfo.vertexDataOffset + offset, SeekOrigin.Begin);
                List<Vector3> verts = new List<Vector3>();
                for (int i = 0; i < tcbInfo.vertexCount; i++)
                {
                    verts.Add(sr.Read<Vector3>());
                }
                vertices = verts;

                //Read main TCB faces
                sr.Seek(tcbInfo.faceDataOffset + offset, SeekOrigin.Begin);
                faces = new List<TCBFace>();
                for (int i = 0; i < tcbInfo.faceCount; i++)
                {
                    faces.Add(sr.Read<TCBFace>());
                }

                //Read main TCB materials
            }
        }

        public unsafe AquaObject ConvertTCBToAquaObject(out AquaNode aqn)
        {
            AquaObject aqo = new AquaObject(createNGSObj: true);

            VTXL vtxl = new VTXL();
            vtxl.vertPositions.AddRange(vertices);
            aqo.vtxlList.Add(vtxl);

            var tris = new GenericTriangles();
            tris.matIdList = new List<int>();
            for (int f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                tris.triList.Add(new Vector3(face.vertex0, face.vertex1, face.vertex2));
                //tris.matIdList.Add(face.materialId);
            }
            tris.matIdList = new List<int>(new int[tris.triList.Count]);
            aqo.tempTris.Add(tris);
            aqo.tempMats.Add(new GenericMaterial() { matName = "TCBMat" });

            //Uncomment when materials are read
            /*
            for(int m = 0; m < tcbModels[i].materials.Count; m++)
            {
                var mat = tcbModels[i].materials[m];
                aqp.tempMats.Add(new AquaObject.GenericMaterial() { matName = $"TCBMat_{m}", diffuseRGBA = new Vector4((float)mat.color[0] / 255, (float)mat.color[1] / 255, (float)mat.color[2] / 255, 1) });
            }*/

            aqn = AquaNode.GenerateBasicAQN();

            aqo.ConvertToPSO2Model(true, false, true, false, true, false, false, false);

            return aqo;
        }

        public struct TCBFace
        {
            public ushort vertex0;
            public ushort vertex1;
            public ushort vertex2;
            public ushort materialId;
        }

        public struct TCBMaterial
        {
            public int field_00; // seems to be 0
            public int field_04; // seems to be 0
            public int field_08; // seems to be 0
            public int field_0C; // seems to be 0
            public int field_10; // seems to be 0
            public int field_14; // seen both 0 and 3
            public fixed byte color[4]; // looks like an RGBA color to me (with alpha as 0)
            public int field_1C; // seems to be 0
        }

        public struct TCB
        {
            public int magic;   //tcb/0 in UTF8
            public int unkInt0;
            public byte flag0;
            public byte flag1;
            public byte flag2;
            public byte flag3;
            public int unkInt1;

            public int vertexDataOffset;
            public int vertexCount;
            public int rel0DataStart;
            public int faceDataOffset;

            public int faceCount;
            public int materialDataOFfset;
            public int materialCount;
            public int nxsMeshOffset;

            public int nxsMeshSize; //Nexus Mesh size in bytes... sometimes 0 for no reason in older files despite a nxsMesh present?
            public int unkInt2;
            public int unkInt3; //Always observed 0x1
            public int unkInt4;
        }
    }
}
