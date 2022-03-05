using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Nova.Structures
{
    public class MeshDefinitions
    {
        public ydbmStruct ydbmStr; //Container header
        public ipnbStruct ipnbStr; //Unknown
        public lxdiStruct lxdiStr; //Face definitions
        public salvStruct salvStr; //Vert definitions
        public AquaObject.VTXE vtxe = new AquaObject.VTXE();
    }
}
