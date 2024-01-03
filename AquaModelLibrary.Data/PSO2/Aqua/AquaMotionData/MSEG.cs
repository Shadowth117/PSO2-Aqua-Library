using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData
{

    //Motion segment. Denotes a node's animations. Camera files will only have one of these. 
    public unsafe struct MSEG
    {
        public int nodeType; //0xE7, type 0x9 
        public int nodeDataCount; //0xE8, type 0x9 
        public int nodeOffset;
        public PSO2String nodeName; //0xE9, type 0x2  
        public int nodeId; //0xEA, type 0x9           //0 on material entries

        public MSEG(List<Dictionary<int, object>> msegRaw)
        {
            nodeType = (int)msegRaw[0][0xE7];
            nodeDataCount = (int)msegRaw[0][0xE8];
            nodeName = new PSO2String((byte[])msegRaw[0][0xE9]);
            nodeId = (int)msegRaw[0][0xEA];

            nodeOffset = 0;
        }

        public byte[] GetBytesVTBF()
        {
            List<byte> outBytes = new List<byte>();

            VTBFMethods.AddBytes(outBytes, 0xE7, 0x9, BitConverter.GetBytes(nodeType));
            VTBFMethods.AddBytes(outBytes, 0xE8, 0x9, BitConverter.GetBytes(nodeDataCount));

            //Node name
            string nodeStr = nodeName.GetString();
            VTBFMethods.AddBytes(outBytes, 0xE9, 0x2, (byte)nodeStr.Length, Encoding.UTF8.GetBytes(nodeStr));
            VTBFMethods.AddBytes(outBytes, 0xEA, 0x9, BitConverter.GetBytes(nodeDataCount));

            VTBFMethods.WriteTagHeader(outBytes, "MSEG", 0x3, 0x4);

            return outBytes.ToArray();
        }
    }
}
