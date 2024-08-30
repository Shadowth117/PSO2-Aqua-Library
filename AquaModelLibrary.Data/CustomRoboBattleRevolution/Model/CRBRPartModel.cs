using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model
{
    public class CRBRPartModel : CRBRModel
    {
        public new CRBRPartModelHeader Header { 
            get { return (CRBRPartModelHeader)base.Header; } 
            set { base.Header = value; } }
        public CRBRPartModel() { }

        public CRBRPartModel(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public CRBRPartModel(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            Header = new CRBRPartModelHeader(sr);
            sr.Seek(Header.offset0 + Header.offset, SeekOrigin.Begin);

            int offset0Offset0 = sr.Read<int>();
            int offset0Offset1 = sr.Read<int>();
            int offset0Unk0 = sr.Read<int>();
            int offset0Unk1 = sr.Read<int>();
            
            int offset0Offset_10 = sr.Read<int>();
            int offset0Unk2 = sr.Read<int>();
            int offset0Unk3 = sr.Read<int>();
            int offset0Unk4 = sr.Read<int>();
        }
    }
}
