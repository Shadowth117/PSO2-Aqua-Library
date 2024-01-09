using AquaModelLibrary.Data.PSO2.Aqua.LobbyActionCommonData;
using AquaModelLibrary.Helpers.Readers;
using System.IO;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class LobbyActionCommon : AquaCommon
    {
        public LacHeader header;
        public DataInfo info;
        public List<DataBlockData> dataBlocks = new List<DataBlockData>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "lac\0"
            };
        }

        public LobbyActionCommon() { }

        public LobbyActionCommon(byte[] file) : base(file) { }

        public LobbyActionCommon(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            header = sr.Read<LacHeader>();

            sr.Seek(header.dataInfoPointer + offset, SeekOrigin.Begin);
            info = sr.Read<DataInfo>();

            sr.Seek(info.blockOffset + offset, SeekOrigin.Begin);

            for (int i = 0; i < info.blockCount; i++)
            {
                dataBlocks.Add(new DataBlockData(sr, offset));
            }
        }

        public struct DataBlock
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

        public class DataBlockData
        {
            public DataBlock rawBlock;

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

            public DataBlockData() { }

            public DataBlockData(BufferedStreamReaderBE<MemoryStream> streamReader, int offset)
            {
                rawBlock = streamReader.Read<DataBlock>();
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
                streamReader.Seek(rawBlock.unkIntOffset0 + offset, System.IO.SeekOrigin.Begin);
                unkOffsetInt0 = streamReader.Read<int>();

                streamReader.Seek(rawBlock.unkIntOffset1 + offset, System.IO.SeekOrigin.Begin);
                unkOffsetInt1 = streamReader.Read<int>();
                streamReader.Seek(rawBlock.unkIntOffset2 + offset, System.IO.SeekOrigin.Begin);
                unkOffsetInt2 = streamReader.Read<int>();
                streamReader.Seek(rawBlock.iceNameOffset + offset, System.IO.SeekOrigin.Begin);
                iceName = streamReader.ReadCString();
                streamReader.Seek(rawBlock.humanAqmOffset + offset, System.IO.SeekOrigin.Begin);
                humanAqm = streamReader.ReadCString();

                streamReader.Seek(rawBlock.castAqmOffset1 + offset, System.IO.SeekOrigin.Begin);
                castAqm1 = streamReader.ReadCString();
                streamReader.Seek(rawBlock.castAqmOffset2 + offset, System.IO.SeekOrigin.Begin);
                castAqm2 = streamReader.ReadCString();
                streamReader.Seek(rawBlock.kmnAqmOffset + offset, System.IO.SeekOrigin.Begin);
                kmnAqm = streamReader.ReadCString();
                streamReader.Seek(rawBlock.vfxOffset + offset, System.IO.SeekOrigin.Begin);
                vfxIce = streamReader.ReadCString();

                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
        }
    }
}
