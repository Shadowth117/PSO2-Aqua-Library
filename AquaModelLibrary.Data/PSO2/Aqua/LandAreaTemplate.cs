using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.AquaStructs
{
    public class LandAreaTemplate : AquaCommon
    {
        public LATHeader header;
        public List<List<LATGridSmallData>> latGridSmallData = new List<List<LATGridSmallData>>(); //Seems to be laid out as the second grid header value as rows with the first's as column count. Could be transposed. Same layout should apply to the below grid values
        public List<List<LATGridData>> latGridData = new List<List<LATGridData>>();

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
                if(pieceId == 0)
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
