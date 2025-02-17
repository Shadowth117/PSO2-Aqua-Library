using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CGPR
{
    public class CGPR
    {
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

            CGPRMagic type0 = (CGPRMagic)sr.Peek<uint>();
            for(int i = 0; i < cgprObjCount; i++)
            {
                switch (type0)
                {
                    case CGPRMagic.xC1A69458:
                        objects.Add(new _C1A69458_Object(sr));
                        break;
                    case CGPRMagic.xFAE88582:
                        objects.Add(new _FAE88582_Object(sr));
                        break;
                    case CGPRMagic.x427AC0E6:
                        objects.Add(new _427AC0E6_Object(sr));
                        break;
                    case CGPRMagic.x2C146841:
                        objects.Add(new _2C146841_Object(sr));
                        break;
                    case CGPRMagic.x7FB9F5F0:
                        objects.Add(new _7FB9F5F0_Object(sr));
                        break;
                    case CGPRMagic.x2FBDFD9B:
                        objects.Add(new _2FBDFD9B_Object(sr));
                        break;
                    default:
                        objects.Add(new CGPRGeneric_Object(sr));
                        break;
                }

                type0 = (CGPRMagic)sr.Peek<uint>();
            }

            var taggedEnd = sr.Position;

            //Read the non tagged objects. There's one of these per tagged object
            for(int i = 0; i < cgprObjCount; i++)
            {
            }
        }
    }
}
