using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.PSO2;
using System.Diagnostics;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class AddOnIndex : AquaCommon
    {
        public List<ADDO> addonList = new List<ADDO>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] { "aox\0" };
        }

        public AddOnIndex() { }

        public AddOnIndex(byte[] file) : base(file) { }

        public AddOnIndex(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadVTBFFile(BufferedStreamReaderBE<MemoryStream> sr)
        {
            //Seek past vtbf tag
            sr.Seek(0x10, SeekOrigin.Current);          //VTBF tags

            while (sr.Position < sr.BaseStream.Length)
            {
                var data = VTBFMethods.ReadVTBFTag(sr, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "DOC ":
                        break;
                    case "ADDO":
                        addonList.Add(new ADDO(data));
                        break;
                    default:
                        //Should mean it's done.
                        Debug.WriteLine($"Defaulted tag was: {tagType}");
                        break;
                }
            }
        }

        public struct ADDO
        {
            public int id;
            public PSO2String leftName;
            public PSO2String leftBoneAttach;
            public PSO2String rightName;
            public PSO2String rightBoneAttach;
            public PSO2String unusedLeftName2;
            public PSO2String unusedLeftBoneAttach2;
            public PSO2String unusedRightName2;
            public PSO2String unusedRightBoneAttach2;
            public byte F8;
            public PSO2String leftEffectName;
            public PSO2String rightEffectName;
            public PSO2String leftEffectAttach;
            public PSO2String rightEffectAttach;
            public byte FD;
            public byte FE;
            public byte E0;
            public PSO2String extraAttach;

            public ADDO(List<Dictionary<int, object>> addoRaw)
            {
                id = VTBFMethods.GetObject<int>(addoRaw[0], 0xFF);
                leftName.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xF0));
                leftBoneAttach.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xF1));
                rightName.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xF2));
                rightBoneAttach.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xF3));
                unusedLeftName2.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xF4));
                unusedLeftBoneAttach2.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xF5));
                unusedRightName2.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xF6));
                unusedRightBoneAttach2.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xF7));
                F8 = VTBFMethods.GetObject<byte>(addoRaw[0], 0xF8);
                leftEffectName.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xF9));
                rightEffectName.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xFA));
                leftEffectAttach.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xFB));
                rightEffectAttach.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xFC));
                FD = VTBFMethods.GetObject<byte>(addoRaw[0], 0xFD);
                FE = VTBFMethods.GetObject<byte>(addoRaw[0], 0xFE);
                E0 = VTBFMethods.GetObject<byte>(addoRaw[0], 0xE0);
                extraAttach.SetBytes(VTBFMethods.GetObject<byte[]>(addoRaw[0], 0xE1));
            }
        }
    }
}
