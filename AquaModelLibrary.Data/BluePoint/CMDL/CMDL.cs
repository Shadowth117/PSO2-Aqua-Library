using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CMDL
{
    public class CMDL
    {
        public CMDLMagic magic;
        public ushort unkUshtCount0;
        public ushort unkUshtCount1;
        public int unkId0;

        //DeSR
        public CVariableTrail unkId0Trail0 = null;
        //SOTC
        public ushort unkId0sht0;
        public ushort unkId0sht1;

        public int unkInt1;

        //DeSR
        public CVariableTrail matTrail = null;
        //SOTC
        public ushort matsht0;
        public ushort matsht1;

        //CMDLs start with a dictionary containing a cmsh material name and a cmat path. This dictionary uses cmsh material name as a key for easy mapping
        public List<CMDL_CMATMaterialMap> cmatReferences = new List<CMDL_CMATMaterialMap>();
        public List<BPString> cmshReferences = new List<BPString>();
        public List<CMDLMeshInfo> cmshInfoList = new List<CMDLMeshInfo>();
        public List<BPString> cpidReferences = new List<BPString>();
        public List<BPString> cclmReferences = new List<BPString>();
        public List<BPString> highQualityCclmReferences = new List<BPString>();
        public List<CMDLClump> clumps = new List<CMDLClump>();

        public CMDL()
        {

        }

        public CMDL(byte[] file)
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
            magic = sr.Read<CMDLMagic>();

            unkUshtCount0 = sr.Read<ushort>();
            unkUshtCount1 = sr.Read<ushort>();
            unkId0 = sr.Read<int>();

            switch (magic)
            {
                case CMDLMagic.SOTC:
                    unkId0sht0 = sr.Read<ushort>();
                    unkId0sht1 = sr.Read<ushort>();
                    break;
                case CMDLMagic.DeSR:
                    unkId0Trail0 = new CVariableTrail(sr);
                    break;
                default:
                    throw new Exception("Unrecognized model type!");
            }
            unkInt1 = sr.Read<int>();

            int matCount;
            switch (magic)
            {
                case CMDLMagic.SOTC:
                    matsht0 = sr.Read<ushort>();
                    matCount = matsht1 = sr.Read<ushort>();
                    break;
                case CMDLMagic.DeSR:
                    matTrail = new CVariableTrail(sr);
                    matCount = matTrail.data[matTrail.data.Count - 1];
                    break;
                default:
                    throw new Exception("Unrecognized model type!");
            }

            for (int i = 0; i < matCount; i++)
            {
                cmatReferences.Add(new CMDL_CMATMaterialMap(sr));
            } 
            
            while (sr.Position < sr.BaseStream.Length - 0x4)
            {
                if (sr.Peek<ushort>() == 0)
                {
                    break;
                }
                var check = sr.Read<uint>();

                switch (check)
                {
                    //CMSHInfo DeSR
                    case 0xE7FC4F9A:
                        cmshInfoList.Add(new CMDLMeshInfo(sr, check));
                        break;
                    //CMSH DeSR
                    case 0x07E055DC:
                    //CMSH SOTC
                    case 0x415D9568:
                        /*
                         * Previous struct prior to the array of cmsh stuff contains count, but we don't necessarily need it
                        var varTrail = clumps[clumps.Count - 1].trail0.data;
                        var count = varTrail[varTrail.Count - 1];
                        */
                        cmshReferences.Add(new BPString(sr));
                        break;
                    //CPID SOTC
                    case 0xA03AC256:
                        cpidReferences.Add(new BPString(sr));
                        break;
                    //CCLM SOTC
                    case 0xB7683EF2:
                        cclmReferences.Add(new BPString(sr));
                        break;
                    //HQ CCLM SOTC
                    case 0xC8C63A73:
                        highQualityCclmReferences.Add(new BPString(sr));
                        break;
                    default:
                        var clump = new CMDLClump(sr, check);
                        clumps.Add(clump);
                        break;
                }
                if (check == 0xE7FC4F9A)
                {
                    break;
                }
            }

        }
    }
}
