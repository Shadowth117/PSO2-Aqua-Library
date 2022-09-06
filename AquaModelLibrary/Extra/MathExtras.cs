using System;
using System.Numerics;

namespace AquaModelLibrary.Extra
{
    public static class MathExtras
    {
        public static Quaternion EulerToQuaternion(Vector3 angle)
        {
            return EulerToQuaternion(angle.X, angle.Y, angle.Z);
        }
        public static Quaternion EulerToQuaternion(double x, double y, double z) // roll (X), pitch (Y),  yaw (Z)
        {
            x *= (float)(Math.PI / 180);
            y *= (float)(Math.PI / 180);
            z *= (float)(Math.PI / 180);

            // Abbreviations for the various angular functions
            double cy = Math.Cos(z * 0.5);
            double sy = Math.Sin(z * 0.5);
            double cp = Math.Cos(y * 0.5);
            double sp = Math.Sin(y * 0.5);
            double cr = Math.Cos(x * 0.5);
            double sr = Math.Sin(x * 0.5);

            Quaternion q = new Quaternion();
            q.W = (float)(cr * cp * cy + sr * sp * sy);
            q.X = (float)(sr * cp * cy - cr * sp * sy);
            q.Y = (float)(cr * sp * cy + sr * cp * sy);
            q.Z = (float)(cr * cp * sy - sr * sp * cy);

            return q;
        }

        //Based on C++ code at https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
        public static Vector3 QuaternionToEuler(Quaternion quat)
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

            angles *= (float)(180 / Math.PI);

            return angles;
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
