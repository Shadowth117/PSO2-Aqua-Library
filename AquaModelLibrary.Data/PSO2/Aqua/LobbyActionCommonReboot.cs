using AquaModelLibrary.Data.PSO2.Aqua.LobbyActionCommonData;
using AquaModelLibrary.Helpers.Readers;
using System.IO;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class LobbyActionCommonReboot : AquaCommon
    {
        public LacHeader header;
        public DataInfo info;
        public List<DataBlockFingersData> rebootDataBlocks = new List<DataBlockFingersData>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "lac\0"
            };
        }

        public LobbyActionCommonReboot() { }

        public LobbyActionCommonReboot(byte[] file) : base(file) { }

        public LobbyActionCommonReboot(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            header = sr.Read<LacHeader>();

            sr.Seek(header.dataInfoPointer + offset, SeekOrigin.Begin);
            info = sr.Read<DataInfo>();

            sr.Seek(info.blockOffset + offset, SeekOrigin.Begin);

            for (int i = 0; i < info.blockCount; i++)
            {
                rebootDataBlocks.Add(new DataBlockFingersData(sr, offset));
            }
        }

        public struct DataBlockReboot
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

        public class DataBlockFingersData
        {
            public DataBlockReboot rawBlock;

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

            public DataBlockFingersData(BufferedStreamReaderBE<MemoryStream> streamReader, int offset)
            {
                rawBlock = streamReader.Read<DataBlockReboot>();
                long bookmark = streamReader.Position;

                unkInt0 = rawBlock.unkInt0;
                streamReader.Seek(rawBlock.internalName0Offset + offset, System.IO.SeekOrigin.Begin);
                internalName0 = streamReader.ReadCString();
                streamReader.Seek(rawBlock.chatCommandOffset + offset, System.IO.SeekOrigin.Begin);
                chatCommand = streamReader.ReadCString();
                streamReader.Seek(rawBlock.internalName1Offset + offset, System.IO.SeekOrigin.Begin);
                internalName1 = streamReader.ReadCString();

                streamReader.Seek(rawBlock.lobbyActionIdOffset + offset, System.IO.SeekOrigin.Begin);
                lobbyActionId = streamReader.ReadCString();
                streamReader.Seek(rawBlock.commonReferenceOffset0 + offset, System.IO.SeekOrigin.Begin);
                commonReference0 = streamReader.ReadCString();
                streamReader.Seek(rawBlock.commonReferenceOffset1 + offset, System.IO.SeekOrigin.Begin);
                commonReference1 = streamReader.ReadCString();
                streamReader.Seek(rawBlock.iceNameOffset + offset, System.IO.SeekOrigin.Begin);
                iceName = streamReader.ReadCString();

                streamReader.Seek(rawBlock.humanAqmOffset + offset, System.IO.SeekOrigin.Begin);
                humanAqm = streamReader.ReadCString();
                streamReader.Seek(rawBlock.castAqm1Offset + offset, System.IO.SeekOrigin.Begin);
                castAqm1 = streamReader.ReadCString();
                streamReader.Seek(rawBlock.castAqm2Offset + offset, System.IO.SeekOrigin.Begin);
                castAqm2 = streamReader.ReadCString();
                streamReader.Seek(rawBlock.kmnAqmOffset + offset, System.IO.SeekOrigin.Begin);
                kmnAqm = streamReader.ReadCString();

                streamReader.Seek(rawBlock.vfxStrOffset + offset, System.IO.SeekOrigin.Begin);
                vfxIce = streamReader.ReadCString();

                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
        }
    }
}
