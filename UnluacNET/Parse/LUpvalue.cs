using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class LUpvalue : BObject
    {
        public int Index { get; set; }
        
        public bool InStack { get; set; }

        public String Name { get; set; }

        public override bool Equals(object obj)
        {
            var upVal = obj as LUpvalue;

            if (upVal != null)
            {
                if (!(InStack == upVal.InStack && Index == upVal.Index))
                {
                    return false;
                }
                else if (Name == upVal.Name)
                {
                    return true;
                }

                return (Name != null && Name == upVal.Name);
            }

            return false;
        }
    }
}
