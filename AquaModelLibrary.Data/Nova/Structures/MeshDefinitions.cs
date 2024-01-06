using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;

namespace AquaModelLibrary.Data.Nova.Structures
{
    public class MeshDefinitions
    {
        public long oaPos = 0; //Position of the mesh group's __oa struct. A bone correlating to this mesh group's data transforms the mesh data before usage ingame. PSO2 and Nova rarely do anything significant with this.
        public ydbmStruct ydbmStr = null; //Container header
        public ipnbStruct lpnbStr = null; //Mesh set bone list
        public ipnbStruct ipnbStr = null; //Mesh bone list
        public lxdiStruct lxdiStr = null; //Face definitions
        public salvStruct salvStr = null; //Vert definitions
        public stamData stam = null; //Material data
        public VTXE vtxe = new VTXE();
    }
}
