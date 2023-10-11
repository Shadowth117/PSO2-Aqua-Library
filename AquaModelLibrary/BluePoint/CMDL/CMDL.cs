using Reloaded.Memory.Streams;
using System.Collections.Generic;

namespace AquaModelLibrary.BluePoint.CMDL
{
    public class CMDL
    {
        public int magic;
        public int unkInt0;
        public int unkId0;
        public CVariableTrail trail0 = null;
        public int unkInt1;

        public CVariableTrail matTrail = null;
        public ushort usht0;
        public byte unkBt0;
        public int unkInt2;

        //CMDLs start with a dictionary containing a cmsh material name and a cmat path. This dictionary uses cmsh material name as a key for easy mapping
        public Dictionary<string, CMDL_CMATMaterialMap> materialDict = new Dictionary<string, CMDL_CMATMaterialMap>();
        public CMDL_CMSHBorder border = null;
        public List<CMDL_CMSHReference> cmshReferences = new List<CMDL_CMSHReference>();

        public CMDL()
        {

        }
        public CMDL(BufferedStreamReader sr)
        {
            magic = sr.Read<int>();
            unkInt0 = sr.Read<int>();
            unkId0 = sr.Read<int>();

            trail0 = new CVariableTrail(sr);

            unkInt1 = sr.Read<int>();
            matTrail = new CVariableTrail(sr);

            for (int i = 0; i < matTrail.data[matTrail.data.Count - 1]; i++)
            {
                var matRef = new CMDL_CMATMaterialMap(sr);
                materialDict.Add(matRef.cmshMaterialName, matRef);
            }
            border = new CMDL_CMSHBorder(sr);

            for (int i = 0; i < border.cmshTrail.data[border.cmshTrail.data.Count - 1]; i++)
            {
                cmshReferences.Add(new CMDL_CMSHReference(sr));
            }
        }
    }
}
