using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.MiscPSO2Structs
{
    public class NexusMesh
    {
        public List<Vector3> vertices;
        public List<Vector3> faces;
        public List<uint> faceIndices;
        public List<NexusBounding> nexusBoundings;
        public List<byte> unkFaceFlags;

        //Vertices should be populated first

        public void GenerateBounding()
        {
            if (vertices != null)
            {
                nexusBoundings = new List<NexusBounding>();

                //There can be multiple sets of boundings in a tcb, but until we determine how we should do that, we'll just do one
                NexusBounding nexusBounding = new NexusBounding();

                Vector3 minVal = new Vector3();
                Vector3 maxVal = new Vector3();
                for (int i = 0; i < vertices.Count; i++)
                {
                    //Compare to max
                    maxVal.X = Math.Max(maxVal.X, vertices[i].X);
                    maxVal.Y = Math.Max(maxVal.Y, vertices[i].Y);
                    maxVal.Z = Math.Max(maxVal.Z, vertices[i].Z);

                    //Compare to min
                    minVal.X = Math.Min(minVal.X, vertices[i].X);
                    minVal.Y = Math.Min(minVal.Y, vertices[i].Y);
                    minVal.Z = Math.Min(minVal.Z, vertices[i].Z);
                }

                float maximum = Math.Max(Math.Abs(maxVal.X), Math.Abs(minVal.X));
                maximum = Math.Max(maximum, Math.Max(Math.Abs(maxVal.Y), Math.Abs(minVal.Y)));
                maximum = Math.Max(maximum, Math.Max(Math.Abs(maxVal.Z), Math.Abs(minVal.Z)));

                maximum *= (float)Math.Pow(2.0, -22.0);

                nexusBounding.epsilon = maximum;
                nexusBounding.minVal = minVal;
                nexusBounding.maxVal = maxVal;
            }
            else
            {
                Console.WriteLine("Vertices are null. Cannot generate bounds.");
            }
        }
        public struct NexusBounding
        {
            public float epsilon;
            public Vector3 minVal;
            public Vector3 maxVal;
        }
    }
}
