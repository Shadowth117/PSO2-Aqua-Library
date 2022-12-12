using System.Collections.Generic;

namespace AquaModelLibrary.Utility
{
    public class AquaUtilData
    {
        public class ModelSet
        {
            public AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
            public List<AquaObject> models = new List<AquaObject>();

            public NGSModelSet GetNGSModelSet()
            {
                var set = new NGSModelSet();
                set.afp = afp;
                set.models = new List<NGSAquaObject>();
                foreach (var mdl in models)
                {
                    set.models.Add((NGSAquaObject)mdl);
                }

                return set;
            }
            public ClassicModelSet GetClassicModelSet()
            {
                var set = new ClassicModelSet();
                set.afp = afp;
                set.models = new List<ClassicAquaObject>();
                foreach (var mdl in models)
                {
                    set.models.Add((ClassicAquaObject)mdl);
                }

                return set;
            }
        }
        public class ClassicModelSet
        {
            public AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
            public List<ClassicAquaObject> models = new List<ClassicAquaObject>();

            public ModelSet GetModelSet()
            {
                var set = new ModelSet();
                set.afp = afp;
                set.models = new List<AquaObject>();
                foreach(var mdl in models)
                {
                    set.models.Add(mdl);
                }

                return set;
            }
        }
        public class NGSModelSet
        {
            public AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
            public List<NGSAquaObject> models = new List<NGSAquaObject>();
            public ModelSet GetModelSet()
            {
                var set = new ModelSet();
                set.afp = afp;
                set.models = new List<AquaObject>();
                foreach (var mdl in models)
                {
                    set.models.Add(mdl);
                }

                return set;
            }
        }

        public class AnimSet
        {
            public AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
            public List<AquaMotion> anims = new List<AquaMotion>();
        }
    }
}
