using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra
{
    public static class MathExtras
    {        
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

            return angles;
        }

        public static double CopySign(double valMain, double valSign)
        {
            double final = Math.Abs(valMain);
            if (valSign >= 0)
            {
                return final;
            }
            else
            {
                return -final;
            }
        }
    }
}
