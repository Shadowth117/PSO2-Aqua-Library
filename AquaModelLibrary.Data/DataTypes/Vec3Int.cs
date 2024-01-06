namespace AquaModelLibrary.Data.DataTypes
{
    public class Vector3Int
    {
        public struct Vec3Int
        {
            public int X;
            public int Y;
            public int Z;

            public Vec3Int(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public int[] GetAsArray()
            {
                return new int[] { X, Y, Z };
            }

            public void SetVec3(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public void SetVec3(Vec3Int oldVec3)
            {
                SetVec3(oldVec3.X, oldVec3.Y, oldVec3.Z);
            }

            public void SetVec3(int[] arr)
            {
                if (arr != null)
                {
                    if (arr.Length > 0)
                    {
                        X = arr[0];
                        if (arr.Length > 1)
                        {
                            Y = arr[1];
                            if (arr.Length > 2)
                            {
                                Z = arr[2];
                            }
                        }
                    }
                }
            }

            public void SetVec3(uint[] arr)
            {
                if (arr != null)
                {
                    if (arr.Length > 0)
                    {
                        X = BitConverter.ToInt32(BitConverter.GetBytes(arr[0]), 0);
                        if (arr.Length > 1)
                        {
                            Y = BitConverter.ToInt32(BitConverter.GetBytes(arr[1]), 0);
                            if (arr.Length > 2)
                            {
                                Z = BitConverter.ToInt32(BitConverter.GetBytes(arr[2]), 0);
                            }
                        }
                    }
                }
            }

            public static Vec3Int CreateVec3Int(int x, int y, int z)
            {
                var vec3 = new Vec3Int();
                vec3.SetVec3(x, y, z);

                return vec3;
            }

            public static Vec3Int CreateVec3Int(Vec3Int oldVec3)
            {
                var vec3 = new Vec3Int();
                vec3.SetVec3(oldVec3.X, oldVec3.Y, oldVec3.Z);

                return vec3;
            }

            public static Vec3Int CreateVec3Int(int[] arr)
            {
                var vec3 = new Vec3Int();
                vec3.SetVec3(arr);

                return vec3;
            }
            public static Vec3Int CreateVec3Int(uint[] arr)
            {
                var vec3 = new Vec3Int();
                vec3.SetVec3(arr);

                return vec3;
            }
        }
    }
}
