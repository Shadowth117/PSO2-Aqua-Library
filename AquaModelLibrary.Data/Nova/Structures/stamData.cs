using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Nova.Structures
{
    //Material data per mesh
    //Variable as hell so really only using this for storage. Could be worked out in order to include much more info.
    public class stamData
    {
        public bool lastStam = false;
        public List<string> texIds = new List<string>();
    }
}
