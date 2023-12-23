using AquaModelLibrary.Extensions.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class LobbyActionCommon : AquaCommon
    {
        public lacHeader header;
        public dataInfo info;
        public List<dataBlockData> dataBlocks = new List<dataBlockData>();
        public List<dataBlockFingersData> rebootDataBlocks = new List<dataBlockFingersData>();

        public struct lacHeader
        {
            public int lacMagic;
            public int reserve0;
            public int reserve1;
            public int reserve2;

            public int dataInfoPointer;
            public int reserve3;
            public int unkInt0;
        }

        public struct dataInfo
        {
            public int blockCount;
            public int blockOffset;
        }

        public struct dataBlock
        {
            public int unkInt0;
            public int internalName0Offset;
            public int chatCommandOffset;
            public int internalName1Offset;

            public int lobbyActionIdOffset;
            public int commonReferenceOffset0; //Used in common.text
            public int commonReferenceOffset1; //?? Not sure the deal with that
            public int unkIntOffset0;          //Human related?

            public int unkIntOffset1;          //Cast male related?
            public int unkIntOffset2;          //Cast female related?
            public int iceNameOffset;
            public int humanAqmOffset;

            public int castAqmOffset1;
            public int castAqmOffset2;
            public int kmnAqmOffset;
            public int vfxOffset;
        }

        public struct dataBlockReboot
        {
            public int unkInt0;
            public int internalName0Offset;
            public int chatCommandOffset;
            public int internalName1Offset;

            public int lobbyActionIdOffset;
            public int commonReferenceOffset0;
            public int commonReferenceOffset1;
            public int iceNameOffset;

            public int humanAqmOffset;
            public int castAqm1Offset;
            public int castAqm2Offset;
            public int kmnAqmOffset;

            public int vfxStrOffset;
        }

        public class dataBlockData
        {
            public dataBlock rawBlock;

            public int unkInt0;
            public string internalName0 = null;
            public string chatCommand = null;
            public string internalName1 = null;

            public string lobbyActionId = null;
            public string commonReference0 = null;
            public string commonReference1 = null;
            public int unkOffsetInt0;

            public int unkOffsetInt1;
            public int unkOffsetInt2;
            public string iceName = null;
            public string humanAqm = null;

            public string castAqm1 = null;
            public string castAqm2 = null;
            public string kmnAqm = null;
            public string vfxIce = null;
        }

        public class dataBlockFingersData
        {
            public dataBlockReboot rawBlock;

            public int unkInt0;
            public string internalName0 = null;
            public string chatCommand = null;
            public string internalName1 = null;

            public string lobbyActionId = null;
            public string commonReference0 = null;
            public string commonReference1 = null;
            public string iceName = null;

            public string humanAqm = null;
            public string castAqm1 = null;
            public string castAqm2 = null;
            public string kmnAqm = null;

            public string vfxIce = null;
        }

        public static dataBlockData ReadDataBlock(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, dataBlock offsetBlock)
        {
            dataBlockData data = new dataBlockData();

            data.rawBlock = offsetBlock;
            data.unkInt0 = offsetBlock.unkInt0;
            streamReader.Seek(offsetBlock.internalName0Offset + offset, System.IO.SeekOrigin.Begin);
            data.internalName0 = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.chatCommandOffset + offset, System.IO.SeekOrigin.Begin);
            data.chatCommand = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.internalName1Offset + offset, System.IO.SeekOrigin.Begin);
            data.internalName1 = streamReader.ReadCString();

            streamReader.Seek(offsetBlock.lobbyActionIdOffset + offset, System.IO.SeekOrigin.Begin);
            data.lobbyActionId = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.commonReferenceOffset0 + offset, System.IO.SeekOrigin.Begin);
            data.commonReference0 = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.commonReferenceOffset1 + offset, System.IO.SeekOrigin.Begin);
            data.commonReference1 = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.unkIntOffset0 + offset, System.IO.SeekOrigin.Begin);
            data.unkOffsetInt0 = streamReader.Read<int>();

            streamReader.Seek(offsetBlock.unkIntOffset1 + offset, System.IO.SeekOrigin.Begin);
            data.unkOffsetInt1 = streamReader.Read<int>();
            streamReader.Seek(offsetBlock.unkIntOffset2 + offset, System.IO.SeekOrigin.Begin);
            data.unkOffsetInt2 = streamReader.Read<int>();
            streamReader.Seek(offsetBlock.iceNameOffset + offset, System.IO.SeekOrigin.Begin);
            data.iceName = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.humanAqmOffset + offset, System.IO.SeekOrigin.Begin);
            data.humanAqm = streamReader.ReadCString();

            streamReader.Seek(offsetBlock.castAqmOffset1 + offset, System.IO.SeekOrigin.Begin);
            data.castAqm1 = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.castAqmOffset2 + offset, System.IO.SeekOrigin.Begin);
            data.castAqm2 = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.kmnAqmOffset + offset, System.IO.SeekOrigin.Begin);
            data.kmnAqm = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.vfxOffset + offset, System.IO.SeekOrigin.Begin);
            data.vfxIce = streamReader.ReadCString();

            return data;
        }

        public static dataBlockFingersData ReadDataBlockReboot(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, dataBlockReboot offsetBlock)
        {
            dataBlockFingersData data = new dataBlockFingersData();

            data.rawBlock = offsetBlock;
            data.unkInt0 = offsetBlock.unkInt0;
            streamReader.Seek(offsetBlock.internalName0Offset + offset, System.IO.SeekOrigin.Begin);
            data.internalName0 = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.chatCommandOffset + offset, System.IO.SeekOrigin.Begin);
            data.chatCommand = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.internalName1Offset + offset, System.IO.SeekOrigin.Begin);
            data.internalName1 = streamReader.ReadCString();

            streamReader.Seek(offsetBlock.lobbyActionIdOffset + offset, System.IO.SeekOrigin.Begin);
            data.lobbyActionId = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.commonReferenceOffset0 + offset, System.IO.SeekOrigin.Begin);
            data.commonReference0 = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.commonReferenceOffset1 + offset, System.IO.SeekOrigin.Begin);
            data.commonReference1 = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.iceNameOffset + offset, System.IO.SeekOrigin.Begin);
            data.iceName = streamReader.ReadCString();

            streamReader.Seek(offsetBlock.humanAqmOffset + offset, System.IO.SeekOrigin.Begin);
            data.humanAqm = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.castAqm1Offset + offset, System.IO.SeekOrigin.Begin);
            data.castAqm1 = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.castAqm2Offset + offset, System.IO.SeekOrigin.Begin);
            data.castAqm2 = streamReader.ReadCString();
            streamReader.Seek(offsetBlock.kmnAqmOffset + offset, System.IO.SeekOrigin.Begin);
            data.kmnAqm = streamReader.ReadCString();

            streamReader.Seek(offsetBlock.vfxStrOffset + offset, System.IO.SeekOrigin.Begin);
            data.vfxIce = streamReader.ReadCString();

            return data;
        }
    }
}
