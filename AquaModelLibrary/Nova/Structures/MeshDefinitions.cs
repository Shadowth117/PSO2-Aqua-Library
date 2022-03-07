using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Nova.Structures
{
    public class MeshDefinitions
    {
        public long oaPos = 0; //Position of the mesh group's __oa struct. A bone correlating to this mesh group's data transforms the mesh data before usage ingame. PSO2 and Nova rarely do anything significant with this.
        public ydbmStruct ydbmStr; //Container header
        public ipnbStruct lpnbStr; //Mesh set bone list
        public ipnbStruct ipnbStr; //Mesh bone list
        public lxdiStruct lxdiStr; //Face definitions
        public salvStruct salvStr; //Vert definitions
        public AquaObject.VTXE vtxe = new AquaObject.VTXE();
    }
}
