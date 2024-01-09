using AquaModelLibrary.Helpers.Readers;
using System.IO;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class LHIObjectDetailLayout : AquaCommon
    {
        public LHIHeader header;
        public IDFloats idFloats;
        public List<DetailInfoObject> detailInfoList = new List<DetailInfoObject>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "lhi\0"
            };
        }

        public LHIObjectDetailLayout() { }

        public LHIObjectDetailLayout(byte[] file) : base(file) { }

        public LHIObjectDetailLayout(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            header = sr.Read<LHIHeader>();

            if (header.idFloatPointer != 0x10 && header.idFloatPointer != 0)
            {
                sr.Seek(offset + header.idFloatPointer, SeekOrigin.Begin);
                idFloats = sr.Read<IDFloats>();
            }

            sr.Seek(offset + header.detailInfoPointer, SeekOrigin.Begin);
            for (int i = 0; i < header.objectTypeCount; i++)
            {
                detailInfoList.Add(new DetailInfoObject(sr, offset));
            }
        }

        public struct LHIHeader
        {
            public int idFloatPointer;
            public int int_4;
            public int int_8;
            public int int_C;

            public int objectTypeCount;
            public int int_14;
            public int int_18;
            public int int_1C;

            public int int_20;
            public int int_24;
            public int detailInfoPointer;
            public int int_2C;
        }

        public struct IDFloats
        {
            public float idFloat_0;  //Odd float, seems to reference area ids, but not the area of the file?
            public float idFloat_4;  //Same as above?
            public float unkFloat_8;
            public float field_C;
        }

        public class DetailInfoObject
        {
            public DetailInfo diStruct;

            public string objName = null;
            public List<Matrix4x4> matrices = new List<Matrix4x4>(); //Matrices are world space

            public DetailInfoObject() { }

            public DetailInfoObject(BufferedStreamReaderBE<MemoryStream> sr, int offset) 
            {
                diStruct = sr.Read<DetailInfo>();

                var bookmark = sr.Position;
                if (diStruct.objNamePointer != 0x10 && diStruct.objNamePointer != 0)
                {
                    sr.Seek(offset + diStruct.objNamePointer, SeekOrigin.Begin);
                    objName = sr.ReadCString();
                }

                sr.Seek(offset + diStruct.matrixArrayPointer, SeekOrigin.Begin);
                for (int i = 0; i < diStruct.matrixArrayCount; i++)
                {
                    matrices.Add(sr.ReadBEMatrix4());
                }

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct DetailInfo
        {
            public int objNamePointer;
            public int int_4;
            public int matrixArrayPointer;
            public int int_C;

            public int matrixArrayCount;
            public Vector3 BoundingMin;
            public Vector3 BoundingMax;
            public float float_2C;
        }
    }
}
