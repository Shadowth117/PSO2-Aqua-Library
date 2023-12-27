using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.PSO2;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
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

        public UNRM() { }

        public UNRM(List<Dictionary<int, object>> unrmRaw)
        {
            if (unrmRaw.Count > 1)
            {
                throw new Exception("Unexpected UNRM size! Please report the offending file!");
            }

            for (int i = 0; i < unrmRaw.Count; i++)
            {
                vertGroupCountCount = (int)unrmRaw[i][0xDA];
                //Read vert group groups
                using (MemoryStream stream = new MemoryStream((byte[])unrmRaw[i][0xDB]))
                using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
                {
                    for (int j = 0; j < vertGroupCountCount; j++)
                    {
                        unrmVertGroups.Add(streamReader.Read<int>());
                    }
                }
                vertCount = (int)unrmRaw[i][0xDC];
                //Read vert mesh ids
                using (MemoryStream stream = new MemoryStream((byte[])unrmRaw[i][0xDD]))
                using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
                {
                    for (int j = 0; j < vertGroupCountCount; j++)
                    {
                        List<int> meshIds = new List<int>();
                        for (int k = 0; k < unrmVertGroups[j]; k++)
                        {
                            meshIds.Add(streamReader.Read<int>());
                        }
                        unrmMeshIds.Add(meshIds);
                    }
                }
                //Read vert ids
                using (MemoryStream stream = new MemoryStream((byte[])unrmRaw[i][0xDE]))
                using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
                {
                    for (int j = 0; j < vertGroupCountCount; j++)
                    {
                        List<int> vertIds = new List<int>();
                        for (int k = 0; k < unrmVertGroups[j]; k++)
                        {
                            vertIds.Add(streamReader.Read<int>());
                        }
                        unrmVertIds.Add(vertIds);
                    }
                }
            }
        }

        public byte[] GetVTBFBytes()
        {
            List<byte> outBytes = new List<byte>();

            //Technically an array so we put the array start tag
            outBytes.AddRange(BitConverter.GetBytes((short)0xFC));

            VTBFMethods.AddBytes(outBytes, 0xDA, 0x9, BitConverter.GetBytes(vertGroupCountCount));

            //unrm vert groups
            outBytes.Add(0xDB); outBytes.Add(0x89);
            if (vertGroupCountCount - 1 > byte.MaxValue)
            {
                if (vertGroupCountCount - 1 > ushort.MaxValue)
                {
                    outBytes.Add(0x18);
                    outBytes.AddRange(BitConverter.GetBytes(vertGroupCountCount - 1));
                }
                else
                {
                    outBytes.Add(0x10);
                    outBytes.AddRange(BitConverter.GetBytes((ushort)(vertGroupCountCount - 1)));
                }
            }
            else
            {
                outBytes.Add(0x08);
                outBytes.Add((byte)(vertGroupCountCount - 1));

            }
            for (int i = 0; i < vertGroupCountCount; i++)
            {
                outBytes.AddRange(BitConverter.GetBytes(unrmVertGroups[i]));
            }

            VTBFMethods.AddBytes(outBytes, 0xDC, 0x9, BitConverter.GetBytes(vertCount));

            //unrm mesh ids
            outBytes.Add(0xDD); outBytes.Add(0x89);
            int meshIDCount = VTBFMethods.GetListOfListOfIntsIntCount(unrmMeshIds);
            if (meshIDCount - 1 > byte.MaxValue)
            {
                if (meshIDCount - 1 > ushort.MaxValue)
                {
                    outBytes.Add(0x18);
                    outBytes.AddRange(BitConverter.GetBytes(meshIDCount - 1));
                }
                else
                {
                    outBytes.Add(0x10);
                    outBytes.AddRange(BitConverter.GetBytes((ushort)(meshIDCount - 1)));
                }
            }
            else
            {
                outBytes.Add(0x08);
                outBytes.Add((byte)(meshIDCount - 1));
            }
            for (int i = 0; i < unrmMeshIds.Count; i++)
            {
                for (int j = 0; j < unrmMeshIds[i].Count; j++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(unrmMeshIds[i][j]));
                }
            }

            //unrm vert ids
            outBytes.Add(0xDE); outBytes.Add(0x89);
            int vertIdCount = VTBFMethods.GetListOfListOfIntsIntCount(unrmVertIds);
            if (vertIdCount - 1 > byte.MaxValue)
            {
                if (vertIdCount - 1 > ushort.MaxValue)
                {
                    outBytes.Add(0x18);
                    outBytes.AddRange(BitConverter.GetBytes(vertIdCount - 1));
                }
                else
                {
                    outBytes.Add(0x10);
                    outBytes.AddRange(BitConverter.GetBytes((ushort)(vertIdCount - 1)));
                }
            }
            else
            {
                outBytes.Add(0x08);
                outBytes.Add((byte)(vertIdCount - 1));
            }
            for (int i = 0; i < unrmVertIds.Count; i++)
            {
                for (int j = 0; j < unrmVertIds[i].Count; j++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(unrmVertIds[i][j]));
                }
            }

            //Technically an array so we put the array end tag
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on UNRM
            //Subtag count. In theory, always 7 for UNRM
            VTBFMethods.WriteTagHeader(outBytes, "UNRM", 0, 7);

            return outBytes.ToArray();
        }

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
            newUnrm.unrmMeshIds = unrmMeshIds.ConvertAll(id => new List<int>((int[])id.ToArray().Clone())).ToList();
            newUnrm.unrmVertIds = unrmVertIds.ConvertAll(id => new List<int>((int[])id.ToArray().Clone())).ToList();

            return newUnrm;
        }
    }
}
