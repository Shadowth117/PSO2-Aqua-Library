using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace SoulsFormats.Formats.Other.MWC
{
    public class OTR : SoulsFile<OTR>
    {
        public OTRHeader header;
        public BoundingBox bounding = null;
        public List<Face> faces = new List<Face>();
        public List<Vertex> vertices = new List<Vertex>();
        public OTR() { }

        public OTR(BinaryReaderEx br) 
        {
            Read(br);
        }

        protected override void Read(BinaryReaderEx br)
        {
            header = new OTRHeader() { int00 = br.ReadInt32(), boundingBoxCount = br.ReadInt32(), faceCount = br.ReadInt32(), vertCount = br.ReadInt32() };

            bounding = new BoundingBox(br);
            br.Position = header.boundingBoxCount * 0x64 + 0x10;

            for (int i = 0; i < header.faceCount; i++)
            {
                faces.Add(new Face()
                {
                    vertIndex0 = br.ReadInt32(),
                    vertIndex1 = br.ReadInt32(),
                    vertIndex2 = br.ReadInt32(),
                    normal = br.ReadVector3(),
                    min = br.ReadVector3(),
                    max = br.ReadVector3(),
                    unkInt0 = br.ReadInt32(),
                    unkInt1 = br.ReadInt32(),
                    unkInt2 = br.ReadInt32(),
                    unkFlt = br.ReadSingle(),
                });
            }

            for (int i = 0; i < header.vertCount; i++)
            {
                vertices.Add(new Vertex()
                {
                    Position = br.ReadVector3(),
                    unkInt = br.ReadInt32(),
                    Color = br.ReadRGBA()
                });
            }
        }

        public struct OTRHeader
        {
            public int int00;
            public int boundingBoxCount;
            public int faceCount; 
            public int vertCount;
        }

        public class BoundingBox
        {
            public BoundingBox() { }
            public BoundingBox(BinaryReaderEx br)
            {
                minBounding = br.ReadVector3();
                maxBounding = br.ReadVector3();
                int18 = br.ReadInt32();
                int1C = br.ReadInt32();
                    
                int20 = br.ReadInt32();
                int24 = br.ReadInt32();
                int28 = br.ReadInt32();
                int2C = br.ReadInt32();
                    
                int30 = br.ReadInt32();
                int34 = br.ReadInt32();
                int38 = br.ReadInt32();
                unkCount1 = br.ReadInt32();

                boxOffset0 = br.ReadInt32();
                boxOffset1 = br.ReadInt32();
                boxOffset2 = br.ReadInt32();
                boxOffset3 = br.ReadInt32();

                unkBt0 = br.ReadByte();
                unkBt1 = br.ReadByte();
                unkBt2 = br.ReadByte();
                unkBt3 = br.ReadByte();
                int54 = br.ReadInt32();
                unk58 = br.ReadInt32();
                unk5C = br.ReadInt32();
                    
                unk60 = br.ReadInt32();

                if(boxOffset0 != 0)
                {
                    box0 = new BoundingBox(br);
                }
                if (boxOffset1 != 0)
                {
                    box1 = new BoundingBox(br);
                }
                if (boxOffset2 != 0)
                {
                    box2 = new BoundingBox(br);
                }
                if (boxOffset3 != 0)
                {
                    box3 = new BoundingBox(br);
                }
            }

            public BoundingBox box0 = null;
            public BoundingBox box1 = null;
            public BoundingBox box2 = null;
            public BoundingBox box3 = null;

            public Vector3 minBounding;
            public Vector3 maxBounding;
            public int int18;
            public int int1C;

            public int int20;
            public int int24;
            public int int28;
            public int int2C;

            public int int30;
            public int int34;
            public int int38;
            public int unkCount1;

            public int boxOffset0;
            public int boxOffset1;
            public int boxOffset2;
            public int boxOffset3;

            public byte unkBt0;
            public byte unkBt1;
            public byte unkBt2;
            public byte unkBt3;
            public int int54;
            public int unk58;
            public int unk5C;

            public int unk60;
        }

        public struct Face
        {
            public int vertIndex0;
            public int vertIndex1;
            public int vertIndex2;
            public Vector3 normal;
            public Vector3 min;
            public Vector3 max;

            public int unkInt0;
            public int unkInt1;
            public int unkInt2;
            public float unkFlt;
        }

        public struct Vertex
        {
            public Vector3 Position;
            public int unkInt;
            public Color Color;
        }
    }
}
