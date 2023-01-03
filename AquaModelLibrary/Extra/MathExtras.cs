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
        ZYX
    }

    public static class MathExtras
    {
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

        public static Quaternion EulerToQuaternion(Vector3 angle, RotationOrder order = RotationOrder.XYZ)
        {
            return EulerToQuaternionRadian(angle * (float)(Math.PI / 180), order);
        }

        private static Quaternion EulerToQuaternionRadian(Vector3 angle, RotationOrder order = RotationOrder.XYZ)
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

        //Based on C++ code at https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
        public static Vector3 QuaternionToEuler(Quaternion quat)
        {
            return QuaternionToEulerRadians(quat) * (float)(180 / Math.PI);
        }

        public static Vector3 QuaternionToEulerRadiansTest(Quaternion quat)
        {
            Vector3 vec3 = new Vector3();

            float sqw = quat.W * quat.W;
            float sqx = quat.X * quat.X;
            float sqy = quat.Y * quat.Y;
            float sqz = quat.Z * quat.Z;
            float unit = sqx + sqy + sqz + sqw; // if normalized is one, otherwise
                                                // is correction factor
            float test = quat.X * quat.Y + quat.Z * quat.W;
            if (test > 0.499 * unit)
            { // singularity at north pole
                vec3.Y = (float)(2 * Math.Atan2(quat.X, quat.W));
                vec3.Z = (float)(Math.PI / 2);
                vec3.X = 0;
            }
            else if (test < -0.499 * unit)
            { // singularity at south pole
                vec3.Y = (float)(-2 * Math.Atan2(quat.X, quat.W));
                vec3.Z = -(float)(Math.PI / 2);
                vec3.X = 0;
            }
            else
            {
                vec3.Y = (float)Math.Atan2(2 * quat.Y * quat.W - 2 * quat.X * quat.Z, sqx - sqy - sqz + sqw); // roll
                                                                                                              // or
                                                                                                              // heading
                vec3.Z = (float)Math.Asin(2 * test / unit); // pitch or attitude
                vec3.X = (float)Math.Atan2(2 * quat.X * quat.W - 2 * quat.Y * quat.Z, -sqx + sqy - sqz + sqw); // yaw
                                                                                                               // or
                                                                                                               // bank
            }
            return vec3;
        }


        public static Vector3 QuaternionToEulerRadians(Quaternion quat)
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

        public static Vector3 QuaternionToEulerRadiansZLimited(Quaternion quat)
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
            if (Math.Abs(siny_cosp) >= 1)
                angles.Z = (float)CopySign(Math.PI / 2, siny_cosp); // use 90 degrees if out of range
            else
                angles.Z = (float)Math.Asin(siny_cosp);

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
