using System.Numerics;

namespace AquaModelLibrary.Helpers.MathHelpers
{
    public enum RotationOrder
    {
        XYZ,
        XZY,
        YXZ,
        YZX,
        ZXY,
        ZYX,
    }

    public static class MathExtras
    {
        public const float basicEpsilon = 0.00001f;
        public const float kEpsilonNormalSqrt = 1e-15F;

        public static Vector3 GetMaximumBounding(Vector3 maxPoint, Vector3 newPoint)
        {
            GetMaximumBounding(ref maxPoint, newPoint);
            return maxPoint;
        }

        public static void GetMaximumBounding(ref Vector3 maxPoint, Vector3 newPoint)
        {
            if (maxPoint.X < newPoint.X)
            {
                maxPoint.X = newPoint.X;
            }
            if (maxPoint.Y < newPoint.Y)
            {
                maxPoint.Y = newPoint.Y;
            }
            if (maxPoint.Z < newPoint.Z)
            {
                maxPoint.Z = newPoint.Z;
            }
        }

        public static Vector3 GetMinimumBounding(Vector3 minPoint, Vector3 newPoint)
        {
            GetMinimumBounding(ref minPoint, newPoint);
            return minPoint;
        }
        public static void GetMinimumBounding(ref Vector3 minPoint, Vector3 newPoint)
        {
            if (minPoint.X > newPoint.X)
            {
                minPoint.X = newPoint.X;
            }
            if (minPoint.Y > newPoint.Y)
            {
                minPoint.Y = newPoint.Y;
            }
            if (minPoint.Z > newPoint.Z)
            {
                minPoint.Z = newPoint.Z;
            }
        }

        /// <summary>
        /// SA Tools implementation
        /// We are using this implementation of Ritter's Bounding Sphere,
        /// because whatever was going on with SharpDX's BoundingSphere class was catastrophically under-shooting
        /// the bounds.
        /// </summary>
        /// <param name="aPoints"></param>
        /// <returns></returns>
        public static void CalculateBoundingSphere(IEnumerable<Vector3> aPoints, out Vector3 center, out float radius)
        {
            Vector3 one = new Vector3(1, 1, 1);

            Vector3 xmin, xmax, ymin, ymax, zmin, zmax;
            xmin = ymin = zmin = one * float.PositiveInfinity;
            xmax = ymax = zmax = one * float.NegativeInfinity;
            foreach (Vector3 p in aPoints)
            {
                if (p.X < xmin.X) xmin = p;
                if (p.X > xmax.X) xmax = p;
                if (p.Y < ymin.Y) ymin = p;
                if (p.Y > ymax.Y) ymax = p;
                if (p.Z < zmin.Z) zmin = p;
                if (p.Z > zmax.Z) zmax = p;
            }
            float xSpan = (xmax - xmin).LengthSquared();
            float ySpan = (ymax - ymin).LengthSquared();
            float zSpan = (zmax - zmin).LengthSquared();
            Vector3 dia1 = xmin;
            Vector3 dia2 = xmax;
            var maxSpan = xSpan;
            if (ySpan > maxSpan)
            {
                maxSpan = ySpan;
                dia1 = ymin; dia2 = ymax;
            }
            if (zSpan > maxSpan)
            {
                dia1 = zmin; dia2 = zmax;
            }
            center = (dia1 + dia2) * 0.5f;
            float sqRad = (dia2 - center).LengthSquared();
            radius = (float)Math.Sqrt(sqRad);

            foreach (var p in aPoints)
            {
                float d = (p - center).LengthSquared();
                if (d > sqRad)
                {
                    var r = (float)Math.Sqrt(d);
                    radius = r;
                    sqRad = radius * radius;
                    var offset = r - radius;
                    center = (radius * center + offset * p) / r;
                }
            }

        }

        public static Vector3 GetFaceNormal(Vector3 vert0, Vector3 vert1, Vector3 vert2)
        {
            //Calculate face normal
            Vector3 u = vert1 - vert0;
            Vector3 v = vert2 - vert0;

            return Vector3.Normalize(Vector3.Cross(u, v));
        }

        // Returns the angle in degrees between /from/ and /to/. This is always the smallest
        public static float Angle(Vector3 from, Vector3 to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = (float)Math.Sqrt(from.GetSquareMagnitude() * to.GetSquareMagnitude());
            if (denominator < kEpsilonNormalSqrt)
                return 0F;

            float dot = Clamp(Vector3.Dot(from, to) / denominator, -1F, 1F);
            var finalValue = (float)(Math.Acos(dot) * (180 / Math.PI));

            return finalValue;
        }

        // Clamps value between min and max and returns value.
        // Set the position of the transform to be that of the time
        // but never less than 1 or more than 3
        //
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        public static bool BoundsIntersect(Vector3 maxBounds, Vector3 minBounds, Vector3 maxBounds2, Vector3 minBounds2, float epsilon)
        {
            bool xTest = CheckAxisBounds(maxBounds.X, minBounds.X, maxBounds2.X, minBounds2.X, epsilon);
            bool yTest = CheckAxisBounds(maxBounds.Y, minBounds.Y, maxBounds2.Y, minBounds2.Y, epsilon);
            bool zTest = CheckAxisBounds(maxBounds.Z, minBounds.Z, maxBounds2.Z, minBounds2.Z, epsilon);
            return xTest && yTest && zTest;
        }

        public static bool CheckAxisBounds(float max0, float min0, float max1, float min1, float epsilon)
        {
            bool max0GreaterThanMin1 = max0.EpsGreaterThanOrEqual(min1, epsilon);
            bool max0GreaterThanMax1 = max0.EpsGreaterThanOrEqual(max1, epsilon);
            bool min0GreaterThanMin1 = min0.EpsGreaterThanOrEqual(min1, epsilon);
            bool max1GreaterThanMin0 = max1.EpsGreaterThanOrEqual(min0, epsilon);
            bool max1GreaterThanMax0 = max1.EpsGreaterThanOrEqual(max0, epsilon);
            bool min1GreaterThanMin0 = min1.EpsGreaterThanOrEqual(min0, epsilon);

            return min1GreaterThanMin0 && max0GreaterThanMin1 ||
                    max1GreaterThanMin0 && max0GreaterThanMax1 ||
                    min0GreaterThanMin1 && max1GreaterThanMin0 ||
                    max0GreaterThanMin1 && max1GreaterThanMax0;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value > max)
            {
                return max;
            }
            else if (value < min)
            {
                return min;
            }

            return value;
        }
        public static float Distance(Vector3 point1, Vector3 point2)
        {
            return (float)Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2) + Math.Pow(point2.Z - point1.Z, 2));
        }

        //Adapted from Threejs https://github.com/mrdoob/three.js/blob/4d6d52aca6fd714fbf0aedb16ce0b8da5701e681/src/math/Matrix4.js
        public static Matrix4x4 Compose(Vector3 position, Quaternion quaternion, Vector3 scale)
        {
            Matrix4x4 mat = new Matrix4x4();

            //Do initial calculations as 64 bit floats for best precision
            double x = quaternion.X, y = quaternion.Y, z = quaternion.Z, w = quaternion.W;
            double x2 = x + x, y2 = y + y, z2 = z + z;
            double xx = x * x2, xy = x * y2, xz = x * z2;
            double yy = y * y2, yz = y * z2, zz = z * z2;
            double wx = w * x2, wy = w * y2, wz = w * z2;

            double sx = scale.X, sy = scale.Y, sz = scale.Z;

            mat.M11 = (float)((1 - (yy + zz)) * sx);
            mat.M12 = (float)((xy + wz) * sx);
            mat.M13 = (float)((xz - wy) * sx);
            mat.M14 = 0;

            mat.M21 = (float)((xy - wz) * sy);
            mat.M22 = (float)((1 - (xx + zz)) * sy);
            mat.M23 = (float)((yz + wx) * sy);
            mat.M24 = 0;

            mat.M31 = (float)((xz + wy) * sz);
            mat.M32 = (float)((yz - wx) * sz);
            mat.M33 = (float)((1 - (xx + yy)) * sz);
            mat.M34 = 0;

            mat.M41 = position.X;
            mat.M42 = position.Y;
            mat.M43 = position.Z;
            mat.M44 = 1;

            return mat;
        }

        public static Matrix4x4 ComposeFromDegreeRotation(Vector3 position, Vector3 eulerRot, Vector3 scale)
        {
            return Compose(position, eulerRot * (float)(Math.PI / 180), scale);
        }

        public static Matrix4x4 Compose(Vector3 position, Vector3 eulerRotRadians, Vector3 scale)
        {
            var matrix = Matrix4x4.Identity;

            matrix *= Matrix4x4.CreateScale(scale);

            var rotation = Matrix4x4.CreateRotationX(eulerRotRadians.X) *
                Matrix4x4.CreateRotationY(eulerRotRadians.Y) *
                Matrix4x4.CreateRotationZ(eulerRotRadians.Z);

            matrix *= rotation;

            matrix *= Matrix4x4.CreateTranslation(position);

            return matrix;
        }

        public static Matrix4x4 ComposeFromDegreeRotationPRS(Vector3 position, Vector3 eulerRot, Vector3 scale)
        {
            return ComposePRS(position, eulerRot * (float)(Math.PI / 180), scale);
        }

        public static Matrix4x4 ComposePRS(Vector3 position, Vector3 eulerRotRadians, Vector3 scale)
        {
            var matrix = Matrix4x4.Identity;

            matrix *= Matrix4x4.CreateTranslation(position);

            var rotation = Matrix4x4.CreateRotationX(eulerRotRadians.X) *
                Matrix4x4.CreateRotationY(eulerRotRadians.Y) *
                Matrix4x4.CreateRotationZ(eulerRotRadians.Z);

            matrix *= rotation;

            matrix *= Matrix4x4.CreateScale(scale);

            return matrix;
        }

        /// <summary>
        /// Assumes euler axes angles are stored in Vector components of the same name. Result is in degrees.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static Quaternion EulerToQuaternion(Vector3 angle, RotationOrder order = RotationOrder.XYZ)
        {
            return EulerToQuaternionRadian(angle * (float)(Math.PI / 180), order);
        }

        /// <summary>
        /// Assumes euler axes angles are stored in Vector components of the same name. Result is in radians.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static Quaternion EulerToQuaternionRadian(Vector3 angle, RotationOrder order = RotationOrder.XYZ)
        {
            float x = angle.X;
            float y = angle.Y;
            float z = angle.Z;
            Matrix4x4 rotation = Matrix4x4.Identity;

            switch (order)
            {
                case RotationOrder.XYZ:
                    rotation = Matrix4x4.CreateRotationX((float)x) *
                        Matrix4x4.CreateRotationY((float)y) *
                        Matrix4x4.CreateRotationZ((float)z);
                    break;
                case RotationOrder.XZY:
                    rotation = Matrix4x4.CreateRotationX((float)x) *
                        Matrix4x4.CreateRotationZ((float)z) *
                        Matrix4x4.CreateRotationY((float)y);
                    break;
                case RotationOrder.YXZ:
                    rotation = Matrix4x4.CreateRotationY((float)y) *
                        Matrix4x4.CreateRotationX((float)x) *
                        Matrix4x4.CreateRotationZ((float)z);
                    break;
                case RotationOrder.YZX:
                    rotation = Matrix4x4.CreateRotationY((float)y) *
                        Matrix4x4.CreateRotationZ((float)z) *
                        Matrix4x4.CreateRotationX((float)x);
                    break;
                case RotationOrder.ZXY:
                    rotation = Matrix4x4.CreateRotationZ((float)z) *
                        Matrix4x4.CreateRotationX((float)x) *
                        Matrix4x4.CreateRotationY((float)y);
                    break;
                case RotationOrder.ZYX:
                    rotation = Matrix4x4.CreateRotationZ((float)z) *
                        Matrix4x4.CreateRotationY((float)y) *
                        Matrix4x4.CreateRotationX((float)x);
                    break;
            }

            Quaternion q = Quaternion.CreateFromRotationMatrix(rotation);

            return q;
        }

        /// <summary>
        /// Assumes euler axes angles are stored in order of the layout name. Result is in degrees.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static Quaternion EulerToQuaternionByOrder(Vector3 angle, RotationOrder order = RotationOrder.XYZ)
        {
            return EulerToQuaternionRadianByOrder(angle * (float)(Math.PI / 180), order);
        }

        /// <summary>
        /// Assumes euler axes angles are stored in order of the layout name. Result is in radians
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static Quaternion EulerToQuaternionRadianByOrder(Vector3 angle, RotationOrder order = RotationOrder.XYZ)
        {
            float x = angle.X;
            float y = angle.Y;
            float z = angle.Z;
            Matrix4x4 rotation = Matrix4x4.Identity;

            switch (order)
            {
                case RotationOrder.XYZ:
                    rotation = Matrix4x4.CreateRotationX((float)x) *
                        Matrix4x4.CreateRotationY((float)y) *
                        Matrix4x4.CreateRotationZ((float)z);
                    break;
                case RotationOrder.XZY:
                    rotation = Matrix4x4.CreateRotationX((float)x) *
                        Matrix4x4.CreateRotationZ((float)y) *
                        Matrix4x4.CreateRotationY((float)z);
                    break;
                case RotationOrder.YXZ:
                    rotation = Matrix4x4.CreateRotationY((float)x) *
                        Matrix4x4.CreateRotationX((float)y) *
                        Matrix4x4.CreateRotationZ((float)z);
                    break;
                case RotationOrder.YZX:
                    rotation = Matrix4x4.CreateRotationY((float)x) *
                        Matrix4x4.CreateRotationZ((float)y) *
                        Matrix4x4.CreateRotationX((float)z);
                    break;
                case RotationOrder.ZXY:
                    rotation = Matrix4x4.CreateRotationZ((float)x) *
                        Matrix4x4.CreateRotationX((float)y) *
                        Matrix4x4.CreateRotationY((float)z);
                    break;
                case RotationOrder.ZYX:
                    rotation = Matrix4x4.CreateRotationZ((float)x) *
                        Matrix4x4.CreateRotationY((float)y) *
                        Matrix4x4.CreateRotationX((float)z);
                    break;
            }

            Quaternion q = Quaternion.CreateFromRotationMatrix(rotation);

            return q;
        }

        // From DSFBX
        public static Vector3 GetFlverEulerFromQuaternion_Bone(Quaternion q)
        {
            // Store the Euler angles in radians
            Vector3 pitchYawRoll = new Vector3();

            double sqw = q.W * q.W;
            double sqx = q.X * q.X;
            double sqy = q.Y * q.Y;
            double sqz = q.Z * q.Z;

            // If quaternion is normalised the unit is one, otherwise it is the correction factor
            double unit = sqx + sqy + sqz + sqw;
            double test = q.X * q.Y + q.Z * q.W;

            if (test > 0.4995f * unit)                              // 0.4999f OR 0.5f - EPSILON
            {
                // Singularity at north pole
                pitchYawRoll.Y = 2f * (float)Math.Atan2(q.X, q.W);  // Yaw
                pitchYawRoll.Z = (float)(Math.PI * 0.5);                         // Pitch
                pitchYawRoll.X = 0f;                                // Roll
                return pitchYawRoll;
            }
            else if (test < -0.4995f * unit)                        // -0.4999f OR -0.5f + EPSILON
            {
                // Singularity at south pole
                pitchYawRoll.Y = -2f * (float)Math.Atan2(q.X, q.W); // Yaw
                pitchYawRoll.Z = (float)-(Math.PI * 0.5);                        // Pitch
                pitchYawRoll.X = 0f;                                // Roll
                return pitchYawRoll;
            }
            else
            {
                pitchYawRoll.Y = (float)Math.Atan2(2f * q.Y * q.W - 2f * q.X * q.Z, sqx - sqy - sqz + sqw);       // Yaw
                pitchYawRoll.Z = (float)Math.Asin(2f * test / unit);                                             // Pitch
                pitchYawRoll.X = (float)Math.Atan2(2f * q.X * q.W - 2f * q.Y * q.Z, -sqx + sqy - sqz + sqw);      // Roll
            }

            return pitchYawRoll;
        }

        public static Vector3 QuaternionToEuler(Quaternion quat, RotationOrder order = RotationOrder.XYZ)
        {
            return QuaternionToEulerRadians(quat, order) * (float)(180 / Math.PI);
        }

        public static Vector3 QuaternionToEulerByOrder(Quaternion quat, RotationOrder order = RotationOrder.XYZ)
        {
            return QuaternionToEulerRadiansByOrder(quat, order) * (float)(180 / Math.PI);
        }

        public static Vector3 QuaternionToEulerRadiansByOrder(Quaternion quat, RotationOrder order = RotationOrder.XYZ)
        {
            var angles = QuaternionToEulerRadians(quat, order);

            //If XYZ, pass through since we already output to that
            float x = angles.X, y = angles.Y, z = angles.Z;
            switch (order)
            {
                case RotationOrder.YXZ:
                    angles.X = y;
                    angles.Y = x;
                    angles.Z = z;
                    break;
                case RotationOrder.ZXY:
                    angles.X = z;
                    angles.Y = x;
                    angles.Z = y;
                    break;
                case RotationOrder.ZYX:
                    angles.X = z;
                    angles.Y = y;
                    angles.Z = x;
                    break;
                case RotationOrder.YZX:
                    angles.X = y;
                    angles.Y = z;
                    angles.Z = x;
                    break;
                case RotationOrder.XZY:
                    angles.X = x;
                    angles.Y = z;
                    angles.Z = y;
                    break;
            }

            return angles;
        }

        public static Vector3 QuaternionToEulerOld(Quaternion quat)
        {
            return QuaternionToEulerRadiansOld(quat) * (float)(180 / Math.PI);
        }

        //Based on C++ code at https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
        //Handles Gimbal Lock on Y axis
        public static Vector3 QuaternionToEulerRadiansOld(Quaternion quat)
        {
            Vector3 angles;

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (quat.W * quat.X + quat.Y * quat.Z);
            double cosr_cosp = 1 - 2 * (quat.X * quat.X + quat.Y * quat.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (quat.W * quat.Y - quat.Z * quat.X);
            if (Math.Abs(sinp) >= 1)
                angles.Y = (float)CopySign(Math.PI / 2, sinp); // use 90 degrees if out of range
            else
                angles.Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (quat.W * quat.Z + quat.X * quat.Y);
            double cosy_cosp = 1 - 2 * (quat.Y * quat.Y + quat.Z * quat.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        public static RotationOrder GetRotationOrder(Quaternion quat)
        {
            double sinr_cosp = 2 * (quat.W * quat.X + quat.Y * quat.Z);
            double sinp = 2 * (quat.W * quat.Y - quat.Z * quat.X);
            double siny_cosp = 2 * (quat.W * quat.Z + quat.X * quat.Y);

            bool xHigh = Math.Abs(sinr_cosp) >= 1;
            bool yHigh = Math.Abs(sinp) >= 1;
            bool zHigh = Math.Abs(siny_cosp) >= 1;

            if (!yHigh)
            {
                return RotationOrder.XYZ;
            }
            else if (!zHigh)
            {
                return RotationOrder.XZY;
            }
            else if (!xHigh)
            {
                return RotationOrder.YXZ;
            }

            return RotationOrder.XYZ;
        }

        //Adapted from https://github.com/mrdoob/three.js/blob/4d6d52aca6fd714fbf0aedb16ce0b8da5701e681/src/math/Euler.js#L105
        public static Vector3 QuaternionToEulerRadians(Quaternion quat, RotationOrder order = RotationOrder.XYZ)
        {
            //We need almost every major bit of logic in the matrix creation for this anyways for any one order, so may as well just prep them all
            var rotMat = Matrix4x4.CreateFromQuaternion(quat);
            Vector3 angles = new Vector3();

            switch (order)
            {
                case RotationOrder.XYZ:
                    angles.Y = (float)Math.Asin(-Clamp(rotMat.M13, -1, 1));

                    if (Math.Abs(rotMat.M13) < 0.9999999)
                    {
                        angles.X = (float)Math.Atan2(rotMat.M23, rotMat.M33);
                        angles.Z = (float)Math.Atan2(rotMat.M12, rotMat.M11);
                    }
                    else
                    {
                        angles.X = (float)Math.Atan2(-rotMat.M32, rotMat.M22);
                        angles.Z = 0;
                    }
                    break;
                case RotationOrder.YXZ:
                    angles.X = (float)Math.Asin(Clamp(rotMat.M23, -1, 1));

                    if (Math.Abs(rotMat.M23) < 0.9999999)
                    {
                        angles.Y = (float)Math.Atan2(-rotMat.M13, rotMat.M33);
                        angles.Z = (float)Math.Atan2(-rotMat.M21, rotMat.M22);
                    }
                    else
                    {
                        angles.Y = (float)Math.Atan2(rotMat.M31, rotMat.M11);
                        angles.Z = 0;
                    }
                    break;
                case RotationOrder.ZXY:
                    angles.X = (float)Math.Asin(-Clamp(rotMat.M32, -1, 1));

                    if (Math.Abs(rotMat.M32) < 0.9999999)
                    {
                        angles.Y = (float)Math.Atan2(rotMat.M31, rotMat.M33);
                        angles.Z = (float)Math.Atan2(rotMat.M12, rotMat.M22);
                    }
                    else
                    {
                        angles.Y = 0;
                        angles.Z = (float)Math.Atan2(-rotMat.M21, rotMat.M11);
                    }
                    break;
                case RotationOrder.ZYX:
                    angles.Y = (float)Math.Asin(Clamp(rotMat.M31, -1, 1));

                    if (Math.Abs(rotMat.M31) < 0.9999999)
                    {
                        angles.X = (float)Math.Atan2(-rotMat.M32, rotMat.M33);
                        angles.Z = (float)Math.Atan2(-rotMat.M21, rotMat.M11);
                    }
                    else
                    {
                        angles.X = 0;
                        angles.Z = (float)Math.Atan2(rotMat.M12, rotMat.M22);
                    }
                    break;
                case RotationOrder.YZX:
                    angles.Z = (float)Math.Asin(-Clamp(rotMat.M21, -1, 1));

                    if (Math.Abs(rotMat.M21) < 0.9999999)
                    {
                        angles.X = (float)Math.Atan2(rotMat.M23, rotMat.M22);
                        angles.Y = (float)Math.Atan2(rotMat.M31, rotMat.M11);
                    }
                    else
                    {
                        angles.X = 0;
                        angles.Y = (float)Math.Atan2(-rotMat.M13, rotMat.M33);
                    }
                    break;
                case RotationOrder.XZY:
                    angles.Z = (float)Math.Asin(Clamp(rotMat.M12, -1, 1));

                    if (Math.Abs(rotMat.M12) < 0.9999999)
                    {
                        angles.X = (float)Math.Atan2(-rotMat.M32, rotMat.M22);
                        angles.Y = (float)Math.Atan2(-rotMat.M13, rotMat.M11);
                    }
                    else
                    {
                        angles.X = (float)Math.Atan2(rotMat.M23, rotMat.M33);
                        angles.Y = 0;
                    }
                    break;
            }

            return angles;
        }

        public static Vector3 QuaternionToEulerTest(Quaternion quat)
        {
            return QuaternionToEulerRadiansTest(quat) * (float)(180 / Math.PI);
        }

        public static Vector3 QuaternionToEulerRadiansTest(Quaternion q)
        {
            Vector3 eulerAngles = new Vector3();

            Quaternion t = new Quaternion(q.X * q.X, q.Y * q.Y, q.Z * q.Z, q.W * q.W);

            float m = t.X + t.Y + t.Z + t.W;
            if (Math.Abs(m) < 0.001d) return eulerAngles;
            float n = 2 * (q.Y * q.W + q.X * q.Z);
            float p = m * m - n * n;

            if (p > 0f)
            {
                eulerAngles.X = (float)Math.Atan2(2.0f * (q.X * q.W - q.Y * q.Z), -t.X - t.Y + t.Z + t.W);
                eulerAngles.Y = (float)Math.Atan2(n, Math.Sqrt(p));
                eulerAngles.Z = (float)Math.Atan2(2.0f * (q.Z * q.W - q.X * q.Y), t.X - t.Y - t.Z + t.W);
            }
            else if (n > 0f)
            {
                eulerAngles.X = 0f;
                eulerAngles.Y = (float)(Math.PI / 2d);
                eulerAngles.Z = (float)Math.Atan2(q.Z * q.W + q.X * q.Y, 0.5f - t.X - t.Y);
            }
            else
            {
                eulerAngles.X = 0f;
                eulerAngles.Y = -(float)(Math.PI / 2d);
                eulerAngles.Z = (float)Math.Atan2(q.Z * q.W + q.X * q.Y, 0.5f - t.X - t.Z);
            }

            return eulerAngles;
        }

        public static Vector3 QuaternionToEulerRadiansNoHandle(Quaternion quat)
        {
            Vector3 angles;

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (quat.W * quat.X + quat.Y * quat.Z);
            double cosr_cosp = 1 - 2 * (quat.X * quat.X + quat.Y * quat.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = Math.Sqrt(1 + 2 * (quat.W * quat.X - quat.Y * quat.Z));
            double cosp = Math.Sqrt(1 - 2 * (quat.W * quat.X - quat.Y * quat.Z));
            angles.Y = (float)(2 * Math.Atan2(sinp, cosp) - Math.PI / 2);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (quat.W * quat.Z + quat.X * quat.Y);
            double cosy_cosp = 1 - 2 * (quat.Y * quat.Y + quat.Z * quat.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        //There's probably a better way to do this, but I am no math king
        public static Matrix4x4 SetMatrixScale(Matrix4x4 mat)
        {
            return SetMatrixScale(mat, new Vector3(1, 1, 1));
        }

        public static Matrix4x4 SetMatrixScale(Matrix4x4 originalMat, Vector3 scaleVec3)
        {
            Matrix4x4 mat;
            Matrix4x4.Decompose(originalMat, out var scaleMat, out var rotMat, out var posMat);
            mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(scaleVec3);
            mat *= Matrix4x4.CreateFromQuaternion(rotMat);
            mat *= Matrix4x4.CreateTranslation(posMat);
            return mat;
        }

        public static double CopySign(double x, double y)
        {
            if (x < 0 && y > 0 || x > 0 && y < 0)
                return -x;
            return x;
        }
        public static bool isEqualVec4(Vector4 a, Vector4 b)
        {
            if (a.X != b.X)
            {
                return false;
            }
            if (a.Y != b.Y)
            {
                return false;
            }
            if (a.Z != b.Z)
            {
                return false;
            }
            if (a.W != b.W)
            {
                return false;
            }

            return true;
        }

    }
}
