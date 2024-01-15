using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Extensions;
using System.Text;
using AquaModelLibrary.Helpers;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class LandAreaTemplate : AquaCommon
    {
        public LATHeader header;
        public List<List<LATGridSmallData>> latGridSmallData = new List<List<LATGridSmallData>>(); //Seems to be laid out as the second grid header value as rows with the first's as column count. Could be transposed. Same layout should apply to the below grid values
        public List<List<LATGridData>> latGridData = new List<List<LATGridData>>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "lat\0"
            };
        }

        public LandAreaTemplate() { }

        public LandAreaTemplate(byte[] file) : base(file) { }

        public LandAreaTemplate(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            header = sr.Read<LATHeader>();

            sr.Seek(offset + header.unkGridIndicesOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.gridHeight; i++)
            {
                var latRow = new List<LATGridSmallData>();
                for (int j = 0; j < header.gridWidth; j++)
                {
                    latRow.Add(sr.Read<LATGridSmallData>());
                }
                latGridSmallData.Add(latRow);
            }

            sr.Seek(offset + header.latGridDataOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.gridHeight; i++)
            {
                var latRow = new List<LATGridData>();
                for (int j = 0; j < header.gridWidth; j++)
                {
                    latRow.Add(sr.Read<LATGridData>());
                }
                latGridData.Add(latRow);
            }
        }

        public override byte[] GetBytesNIFL()
        {
            List<byte> outBytes = new List<byte>();
            foreach(var row in latGridSmallData)
            {
                foreach(var data in row)
                {
                    outBytes.AddRange(DataHelpers.ConvertStruct(data));
                }
            }
            foreach (var row in latGridData)
            {
                foreach (var data in row)
                {
                    outBytes.AddRange(DataHelpers.ConvertStruct(data));
                }
            }

            return outBytes.ToArray();
        }

        public struct LATGridData
        {
            public string pieceIdText
            {
                get { return GetIdAsString(); }
            }

            public ushort pieceId;      //Correlates to pieceId
            public ushort usht_02;
            public ushort usht_04;
            public ushort usht_06;
            public ushort usht_08;
            public ushort usht_0A;
            public ushort usht_0C;
            public ushort usht_0E;

            public ushort usht_10;
            public ushort usht_12;
            public ushort usht_14;
            public ushort usht_16;
            public ushort usht_18;
            public ushort usht_1A;
            public ushort usht_1C;
            public ushort usht_1E;

            public ushort usht_20;
            public ushort usht_22;

            public string GetIdAsString()
            {
                if (pieceId == 0)
                {
                    return null;
                }
                return Encoding.UTF8.GetString(BitConverter.GetBytes(pieceId));
            }
        }

        public struct LATGridSmallData
        {
            public byte id; //id relating to the tile type?
            public byte bt_1;
            public byte bt_2;
            public byte bt_3;
        }

        //There's junk after this and like most NIFL files, it looks like debug values
        public struct LATHeader
        {
            public int gridHeight; //Could be reversed
            public int gridWidth;
            public int unk_08;
            public int unkGridIndicesOffset;

            public int latGridDataOffset;
            public int reserve0;
        }
    }
}
