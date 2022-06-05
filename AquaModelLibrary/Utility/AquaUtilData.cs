using System.Collections.Generic;

namespace AquaModelLibrary.Utility
{
    public class AquaUtilData
    {
        public class ModelSet
        {
            public AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
            public List<AquaObject> models = new List<AquaObject>();
        }

        public class AnimSet
        {
            public AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
            public List<AquaMotion> anims = new List<AquaMotion>();
        }
    }
}
