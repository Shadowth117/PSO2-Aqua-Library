using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary
{
    //Props to DeadlyFugu for much of this class
    public unsafe class TCBTerrainConvex
    {
        public List<Vector3> vertices;
        public List<TCBFace> faces;
        public List<TCBMaterial> materials;
        public NexusMesh nxsMesh;
        public TCB tcbInfo;

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
