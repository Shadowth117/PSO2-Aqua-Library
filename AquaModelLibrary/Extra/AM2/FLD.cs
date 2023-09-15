using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.AM2
{
    //Stage collision model?
    public class FLD
    {
        public List<long> modelOffsets = new List<long>(); //Always 0x10 of padding after alignment following these. No count variable
        public List<FLDModel> fldModels = new List<FLDModel>();

        public FLD()
        {

        }

        public FLD(BufferedStreamReader sr)
        {
            //Shouldn't be too many lines, but fallback just in case
            while (sr.Position() < sr.BaseStream().Length && sr.Peek<long>() != 0)
            {
                modelOffsets.Add(sr.Read<long>());
            }

            foreach (var offset in modelOffsets)
            {
                sr.Seek(offset, System.IO.SeekOrigin.Begin);
                fldModels.Add(new FLDModel(sr));
            }
        }

        public class FLDModel
        {
            public AquaCommon.PSO2String modelName;
            public FLDModelOffsets dataOffsets;
            public FLDModelHeader0 modelHeader0;
            public FLDModelHeader1 modelHeader1;
            public List<long> shortListOffsets = new List<long>();
            public List<List<short>> shortListLists = new List<List<short>>();
            public List<Vector3> vertPositions = new List<Vector3>();
            public List<Vector3> vertNormals = new List<Vector3>();
            public List<float> vertUnkFloats = new List<float>();
            public List<FLDPolygon> polygons = new List<FLDPolygon>();

            public FLDModel() { }

            public FLDModel(BufferedStreamReader sr)
            {
                dataOffsets = sr.Read<FLDModelOffsets>();
                
                sr.Seek(dataOffsets.modelHeader0Offset, System.IO.SeekOrigin.Begin);
                modelHeader0 = sr.Read<FLDModelHeader0>();

                sr.Seek(dataOffsets.modelHeader1Offset, System.IO.SeekOrigin.Begin);
                modelHeader1 = sr.Read<FLDModelHeader1>();

                sr.Seek(modelHeader1.modelNameOffset, System.IO.SeekOrigin.Begin);
                modelName = sr.Read<AquaCommon.PSO2String>();

                if(dataOffsets.shortListsOffsetsOffset != 0)
                {
                    sr.Seek(dataOffsets.shortListsOffsetsOffset, System.IO.SeekOrigin.Begin);
                    for(int i = 0; i < modelHeader0.shortListOffsetsCount; i++)
                    {
                        shortListOffsets.Add(sr.Read<long>());
                    }
                }

                //A lot of this can end up duplicated. Could probably optimize this and would definitely want to for export, but it's not a huge problem.
                foreach(var offset in shortListOffsets)
                {
                    var list = new List<short>();
                    sr.Seek(offset, System.IO.SeekOrigin.Begin);
                    while(sr.Position() < sr.BaseStream().Length && sr.Peek<short>() != -1)
                    {
                        list.Add(sr.Read<short>());
                    }
                    shortListLists.Add(list);
                }

                //Vert positions
                sr.Seek(dataOffsets.vertPositionsOffset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < modelHeader1.vertexCount; i++)
                {
                    vertPositions.Add(sr.Read<Vector3>());
                }

                //Vert normals
                sr.Seek(dataOffsets.vertNormalsOffset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < modelHeader1.vertexCount; i++)
                {
                    vertNormals.Add(sr.Read<Vector3>());
                }

                //Vert unknowns
                sr.Seek(dataOffsets.vertNormalsOffset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < modelHeader1.vertexCount; i++)
                {
                    vertUnkFloats.Add(sr.Read<float>());
                }

                //Polygons
                sr.Seek(dataOffsets.polygonIndicesOffset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < modelHeader1.polygonCount; i++)
                {
                    polygons.Add(sr.Read<FLDPolygon>());
                }

            }
        }

        public struct FLDPolygon
        {
            public byte flag0;
            public byte flag1;
            public byte flag2;
            public byte flag3;
            public ushort unkId0;
            public ushort unkId1;
            public ushort vertId0;
            public ushort vertId1;
            public ushort vertId2;
            public ushort vertId3;
        }

        public struct FLDModelOffsets
        {
            public long vertPositionsOffset;
            public long vertNormalsOffset;
            public long vertUnknownFloatsOffset;
            public long polygonIndicesOffset;

            public long modelHeader0Offset;
            public long shortListsOffsetsOffset;
            public long modelHeader1Offset;
        }

        public struct FLDModelHeader0
        {
            public Vector4 vec4_00;
            public int unkCount_10;
            public int unkCount_14;
            public int unkCount_18;
            public int shortListOffsetsCount;
            public Vector4 vec4_20;
        }
        public struct FLDModelHeader1
        {
            public int polygonCount;
            public int vertexCount;
            public Vector4 vec4_08;
            public long modelNameOffset;
        }

    }
}
