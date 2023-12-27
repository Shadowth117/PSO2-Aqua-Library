using AquaModelLibrary.Helpers.Readers;
using System.Collections.Generic;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
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

        public TSET(Dictionary<int, object> tsetRaw)
        {
            unkInt0 = (int)tsetRaw[0x70];
            texCount = (int)tsetRaw[0x71];
            unkInt1 = (int)tsetRaw[0x72];
            unkInt2 = (int)tsetRaw[0x73];
            unkInt3 = (int)tsetRaw[0x74];

            //Read tsta texture IDs
            using (MemoryStream stream = new MemoryStream((byte[])tsetRaw[0x75]))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                for (int j = 0; j < 4; j++)
                {
                    tstaTexIDs.Add(streamReader.Read<int>());
                }
            }
        }

        public TSET(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            Read(streamReader);
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

        public void Read(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            unkInt0 = streamReader.Read<int>();
            texCount = streamReader.Read<int>();
            unkInt1 = streamReader.Read<int>();
            unkInt2 = streamReader.Read<int>();

            unkInt3 = streamReader.Read<int>();

            long structEnd = streamReader.Position + 0x10;
            //This section will be the classic int based indexing if 4 or less textures (0 is a valid count). 0xFFFFFFFF signifies a null texture.
            //If this section has more textures than 4, ids will be listed in bytes with remainder filled in by 0xFF.
            if (texCount > 4)
            {
                for (int i = 0; i < texCount; i++)
                {
                    byte temp = streamReader.Read<byte>();
                    if (temp != 0xFF)
                    {
                        tstaTexIDs.Add(temp);
                    }
                }
            }
            else
            {
                for (int i = 0; i < texCount; i++)
                {
                    int temp = streamReader.Read<int>();
                    if (temp >= 0)
                    {
                        tstaTexIDs.Add(temp);
                    }
                }
            }
            streamReader.Seek(structEnd, SeekOrigin.Begin);
        }
        public static TSET ReadTSET(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            TSET tset = new TSET();
            tset.Read(streamReader);
            return tset;
        }
    }
}
