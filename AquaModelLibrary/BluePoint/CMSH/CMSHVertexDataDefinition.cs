using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CMSH
{
    public class CMSHVertexDataDefinition
    {
        public VertexMagic dataMagic; //0SOP for POS0 for position data, etc.
        public ushort dataFormat; //Formal definition of above?
        public ushort usht_06;
        public int dataStart; //Based from where the size starts counting for VertexData
        public int int_0C;

        public int dataSize; //Size of this data's buffer
        public int int_14;
    }
}
