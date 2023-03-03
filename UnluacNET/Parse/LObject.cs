using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public abstract class LObject : BObject
    {
        public abstract new bool Equals(Object obj);

        public virtual string DeRef()
        {
            throw new NotImplementedException();
        }
    }
}
