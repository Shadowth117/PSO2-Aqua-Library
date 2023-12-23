using AquaModelLibrary.Helpers.MathHelpers;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    public struct BoundingVolume
    {
        public Vector3 modelCenter; //0x1E, Type 0x4A, Count 0x1
        public float reserve0;
        public float boundingRadius; //0x1F, Type 0xA                    //Distance of furthest point from the origin
        public Vector3 modelCenter2; //0x20, Type 0x4A, Count 0x1        //Model Center... again 
        public float reserve1;
        public Vector3 halfExtents; //0x21, Type 0x4A, Count 0x1 //Distance between max/min of x, y and z divided by 2

        public BoundingVolume(List<VTXL> vertData)
        {
            Vector3 maxPoint = new Vector3();
            Vector3 minPoint = new Vector3();
            Vector3 difference = new Vector3();
            Vector3 center = new Vector3();
            float radius = 0;

            for (int vset = 0; vset < vertData.Count; vset++)
            {
                for (int vert = 0; vert < vertData[vset].vertPositions.Count; vert++)
                {
                    //Compare to max
                    maxPoint = MathExtras.GetMaximumBounding(maxPoint, vertData[vset].vertPositions[vert]);

                    //Compare to min
                    minPoint = MathExtras.GetMinimumBounding(minPoint, vertData[vset].vertPositions[vert]);
                }
            }

            difference.X = Math.Abs(maxPoint.X - minPoint.X / 2);
            difference.Y = Math.Abs(maxPoint.Y - minPoint.Y / 2);
            difference.Z = Math.Abs(maxPoint.Z - minPoint.Z / 2);
            center.X = maxPoint.X - difference.X;
            center.Y = maxPoint.Y - difference.Y;
            center.Z = maxPoint.Z - difference.Z;

            //Get max radius from center
            for (int vset = 0; vset < vertData.Count; vset++)
            {
                for (int vert = 0; vert < vertData[vset].vertPositions.Count; vert++)
                {
                    float distance = MathExtras.Distance(center, vertData[vset].vertPositions[vert]);
                    if (distance > radius)
                    {
                        radius = distance;
                    }
                }
            }

            modelCenter = center;
            reserve0 = 0;
            modelCenter2 = center;
            halfExtents = difference;
            reserve1 = 0;
            boundingRadius = radius;
        }
    }
}
