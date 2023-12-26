using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.PSO2;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class BCLNObject
    {
        public BCLN bcln;
        public BCLNRitem bclnRitem;
        public BCLNRitem2 bclnRitem2;

        public BCLNObject() { }

        public BCLNObject(List<Dictionary<int, object>> lnRaw)
        {
            bcln = new BCLN();
            bcln.id = (int)lnRaw[0][0xFF];
            bcln.fileId = VTBFMethods.GetObject<int>(lnRaw[0], 0x1);
            bcln.unkInt = VTBFMethods.GetObject<int>(lnRaw[0], 0x2);
        }

        public byte[] GetBytes(int mode)
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(DataHelpers.ConvertStruct(bcln));
            if (mode >= 1)
            {
                outBytes.AddRange(DataHelpers.ConvertStruct(bclnRitem));
                outBytes.AddRange(DataHelpers.ConvertStruct(bclnRitem2));
            }

            return outBytes.ToArray();
        }
    }

    //Also for LCLN, ACLN, and ICLN. Id remapping info. Recolor outfits have multiple ids, but only one id that will actually correlate to a file.
    public struct BCLN
    {
        public int id;
        public int fileId;
        public int unkInt;
    }

    public struct BCLNRitem
    {
        public int int_00;
        public int int_04;
        public int int_08;
        public int int_0C;
    }

    //Added sometime before April 3 2022
    public struct BCLNRitem2
    {
        public int int_00;
        public int int_04;
    }
}
