using System;
using System.Numerics;

namespace AquaModelLibrary.Extra
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
        public static bool EpsEqual(this float flt, float otherFloat, float epsilon)
        {
            return otherFloat <= flt + epsilon && otherFloat >= flt - epsilon;
        }

        public static bool EpsGreaterThanOrEqual(this float flt, float otherFloat, float epsilon)
        {
            return flt > otherFloat || EpsEqual(flt, otherFloat, epsilon);
        }

        public static bool EpsLessThanOrEqual(this float flt, float otherFloat, float epsilon)
        {
            return flt < otherFloat || EpsEqual(flt, otherFloat, epsilon);
        }

        public static bool EpsEqual(this Vector3 vec3, Vector3 otherVec3, float epsilon)
        {
            return EpsEqual(vec3.X, otherVec3.X, epsilon) && EpsEqual(vec3.Y, otherVec3.Y, epsilon) && EpsEqual(vec3.Z, otherVec3.Z, epsilon);
        }

        public static bool GreaterThanOrEqualToPoint(this Vector3 vec3, Vector3 boundingVec3, float epsilon)
        {
            return vec3.X.EpsGreaterThanOrEqual(boundingVec3.X, epsilon) && vec3.Y.EpsGreaterThanOrEqual(boundingVec3.Y, epsilon) && vec3.Z.EpsGreaterThanOrEqual(boundingVec3.Z, epsilon);
        }
        public static bool LessThanOrEqualToPoint(this Vector3 vec3, Vector3 boundingVec3, float epsilon)
        {
            return vec3.X.EpsLessThanOrEqual(boundingVec3.X, epsilon) && vec3.Y.EpsLessThanOrEqual(boundingVec3.Y, epsilon) && vec3.Z.EpsLessThanOrEqual(boundingVec3.Z, epsilon);
        }

        public static bool WithinBounds(this Vector3 point, Vector3 maxBounds, Vector3 minBounds, float epsilon)
        {
            return point.GreaterThanOrEqualToPoint(minBounds, epsilon) && point.LessThanOrEqualToPoint(maxBounds, epsilon);
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
            bool max0GreaterThanMin1 = EpsGreaterThanOrEqual(max0, min1, epsilon);
            bool max0GreaterThanMax1 = EpsGreaterThanOrEqual(max0, max1, epsilon);
            bool min0GreaterThanMin1 = EpsGreaterThanOrEqual(min0, min1, epsilon);
            bool max1GreaterThanMin0 = EpsGreaterThanOrEqual(max1, min0, epsilon);
            bool max1GreaterThanMax0 = EpsGreaterThanOrEqual(max1, max0, epsilon);
            bool min1GreaterThanMin0 = EpsGreaterThanOrEqual(min1, min0, epsilon);

            return ((min1GreaterThanMin0 && max0GreaterThanMin1) ||
                    (max1GreaterThanMin0 && max0GreaterThanMax1) ||
                    (min0GreaterThanMin1 && max1GreaterThanMin0) ||
                    (max0GreaterThanMin1 && max1GreaterThanMax0));
        }

        public static float Clamp(float value, float min, float max)
        {
            if(value > max)
            {
                return max;
            } else if (value < min)
            {
                return min;
            }

            return value;
        }
        public static float Distance(Vector3 point1, Vector3 point2)
        {
            return (float)Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2) + Math.Pow(point2.Z - point1.Z, 2));
        }

        public static void MirrorX(this Quaternion quat)
        {
            quat.Y = -quat.Y;
            quat.Z = -quat.Z;
        }

        public static void MirrorY(this Quaternion quat)
        {
            quat.X = -quat.X;
            quat.Z = -quat.Z;
        }
        public static void MirrorZ(this Quaternion quat)
        {
            quat.X = -quat.X;
            quat.Y = -quat.Y;
        }

        public static Quaternion ToQuat(this Vector4 vec4)
        {
            return new Quaternion(vec4.X, vec4.Y, vec4.Z, vec4.W);
        }

        public static Vector4 ToVec4(this Quaternion quat)
        {
            return new Vector4(quat.X, quat.Y, quat.Z, quat.W);
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
            switch(order)
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
                    } else
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
            if ((x < 0 && y > 0) || (x > 0 && y < 0))
                return -x;
            return x;
        }
        public static float Get(this Vector3 vec3, int id)
        {
            switch (id)
            {
                case 0:
                    return vec3.X;
                case 1:
                    return vec3.Y;
                case 2:
                    return vec3.Z;
                default:
                    throw new Exception("Unreachable Code");
            }
        }
        public static void Set(this Vector3 vec3, int id, float value)
        {
            switch (id)
            {
                case 0:
                    vec3.X = value;
                    break;
                case 1:
                    vec3.Y = value;
                    break;
                case 2:
                    vec3.Z = value;
                    break;
            }
        }
        public static float Get(this Vector4 vec4, int id)
        {
            switch (id)
            {
                case 0:
                    return vec4.X;
                case 1:
                    return vec4.Y;
                case 2:
                    return vec4.Z;
                case 3:
                    return vec4.W;
                default:
                    throw new Exception("Unreachable Code");
            }
        }
        public static void Set(this Vector4 vec4, int id, float value)
        {
            switch (id)
            {
                case 0:
                    vec4.X = value;
                    break;
                case 1:
                    vec4.Y = value;
                    break;
                case 2:
                    vec4.Z = value;
                    break;
                case 3:
                    vec4.W = value;
                    break;
            }
        }
    }
}
