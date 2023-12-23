using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class AttachTransformObject
    {
        public AttachTransform attach;
        public string name = null;
        public string unkText = null;
        public string attachNode = null;
    }

    public struct AttachTransform
    {
        public int namePtr;
        public int unkTextPtr;
        public int attachNodePtr;
        public Vector3 pos;
        public Vector3 rot;
        public float scale;
        public int unkInt;
    }
}
