using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class ColliderObject
    {
        public Collider colStruct;
        public string name = null;
        public string text1 = null;
        public ColliderObject() { }

        public ColliderObject(BufferedStreamReaderBE<MemoryStream> streamReader, int ptr, int offset)
        {
            streamReader.Seek(offset + ptr, SeekOrigin.Begin);
            colStruct = streamReader.Read<Collider>();
            name = streamReader.ReadCStringValidOffset(colStruct.namePtr, offset);
            text1 = streamReader.ReadCStringValidOffset(colStruct.text1Ptr, offset);
        }
    }

    public struct Collider
    {
        public int namePtr;
        public int shape; //0 - Sphere: p0=radius; (origin at center)
                          //1 - Cylinder: p0=radius, p1=height; (origin at bottom. Seems to be rotated 90 X, 90 Z going by 3ds Max's standard Z-Up)
                          //2 - Box: p0=width1, p2=width2, p3=width3; (origin at center)
                          //3 - Plane: p0=width1, p1=width2; (Backface culled. Origin is its center on one edge)
                          //4 - Cone: p0=radiusUp, p1=radiusDown, p2=height; (origin at bottom. Seems to be rotated 90 X, 90 Z going by 3ds Max's standard Z-Up)
                          //5 - Pyramid: p0=edge, p1=height; (rotated 45 degrees with an edge facing forward and top point at shape origin) 
                          //6+ has no shape and cannot be damaged.
        public float shapeParam0;
        public float shapeParam1;

        public float shapeParam2;
        public int text1Ptr;
        public float shapeParam3;
        public float shapeParam4;

        public float shapeParam5;
        public float shapeParam6;
        public float shapeParam7;
        public float shapeParam8;
    }
}
