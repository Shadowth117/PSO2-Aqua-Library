using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CGPR
{
    public class CGPR
    {
        public BPEra era;
        public List<CGPRObject> objects = new List<CGPRObject>();
        public CFooter cfooter;
        public int cgprObjCount;
        public int int_04; //For 0 count cgpr

        public CGPR()
        {

        }
        public CGPR(byte[] file)
        {
            file = CompressionHandler.CheckCompression(file);
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var cgprObjCount = sr.Read<int>();

            //Should just be an empty cgpr
            if (cgprObjCount == 0)
            {
                int_04 = sr.Read<int>();
                cfooter = sr.Read<CFooter>();
            }

            era = BPEra.None;
            for(int i = 0; i < cgprObjCount; i++)
            {
                objects.Add(CGPRObject.ReadObject(sr, era));
                if(i == 0 && objects[i] != null)
                {
                    era = objects[i].era;
                }
            }

            var taggedEnd = sr.Position;

            //Read the non tagged objects. There's one of these per tagged object
            for(int i = 0; i < cgprObjCount; i++)
            {
            }
        }
    }
}
