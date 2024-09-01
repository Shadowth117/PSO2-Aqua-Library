using AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model
{
    public class CRBRPartModel : CRBRModel
    {
        public List<CRBRModelDataSet> modelDataList = new List<CRBRModelDataSet>();
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
            sr.Seek(Header.modelDataListOffset + Header.offset, SeekOrigin.Begin);

            for(int i = 0; i < Header.modelDataListCount; i++)
            {
                var bookmark = sr.Position;
                modelDataList.Add(new CRBRModelDataSet(sr, Header.offset, this));
                sr.Seek(bookmark + 0x10, SeekOrigin.Begin);
            }
        }
    }
}
