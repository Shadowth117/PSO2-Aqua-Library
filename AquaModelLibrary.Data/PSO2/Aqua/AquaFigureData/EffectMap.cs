using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    //Effect mappings have numerous structures based on the type id. 
    //For sanity purposes, these will be stored in lists since there are only 3 distinct data types known to exist in them, though more may actually be used.
    //For writeback, note that data is read in by order. 
    public class EffectMapObject
    {
        public int type;
        public List<int> intList = new List<int>();
        public List<float> fltList = new List<float>();
        public List<string> strList = new List<string>();
        public List<int> colorList = new List<int>();

        //Extra
        public bool knownType;

        public EffectMapObject() { }

        public EffectMapObject(int offset, long address, BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr.Seek(offset + address, SeekOrigin.Begin);
            type = sr.Read<int>();

            //All of these are mapped a bit arbitrarily and need externally defined mappings to be read
            if (FigEffectMapStructs.effectMappings.ContainsKey(type))
            {
                int[] map = FigEffectMapStructs.effectMappings[type];
                foreach (int str in map)
                {
                    switch (str)
                    {
                        case 0:
                            intList.Add(sr.Read<int>());
                            break;
                        case 1:
                            fltList.Add(sr.Read<float>());
                            break;
                        case 2:
                            int stringPtr = sr.Read<int>();
                            string effString = sr.ReadCStringValidOffset(stringPtr, offset);
                            strList.Add(effString);
                            break;
                        case 3: //Unsure if an actual type
                            colorList.Add(sr.Read<int>());
                            break;
                    }
                }
                knownType = true;
            }
            else
            {
                knownType = false;
            }
        }
    }
    public struct StateMapping
    {
        public int namePtr;
        public int commandPtr;
        public int effectMapPtr;
        public int animMapPtr;
        public int commandCount;
        public int effectMapCount;
        public int animMapCount;
    }
}
