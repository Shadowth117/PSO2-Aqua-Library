using System.Numerics;

namespace AquaModelLibrary.Helpers.MathHelpers
{
    public static class VectorExtensions
    {
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

        public static Vector3 Set(Vector3 vec3, int id, float value)
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

            return vec3;
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

        public static Vector4 Set(Vector4 vec4, int id, float value)
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

            return vec4;
        }

        public static Vector4 AddToId(Vector4 vec4, float value, int id)
        {
            switch (id)
            {
                case 0:
                    vec4.X += value;
                    break;
                case 1:
                    vec4.Y += value;
                    break;
                case 2:
                    vec4.Z += value;
                    break;
                case 3:
                    vec4.W += value;
                    break;
            }

            return vec4;
        }

        public static Quaternion MirrorX(Quaternion quat)
        {
            quat.Y = -quat.Y;
            quat.Z = -quat.Z;

            return quat;
        }

        public static Quaternion MirrorY(Quaternion quat)
        {
            quat.X = -quat.X;
            quat.Z = -quat.Z;

            return quat;
        }

        public static Quaternion MirrorZ(Quaternion quat)
        {
            quat.X = -quat.X;
            quat.Y = -quat.Y;

            return quat;
        }

        public static Quaternion ToQuat(this Vector4 vec4)
        {
            return new Quaternion(vec4.X, vec4.Y, vec4.Z, vec4.W);
        }

        public static Vector4 ToVec4(this Quaternion quat)
        {
            return new Vector4(quat.X, quat.Y, quat.Z, quat.W);
        }

        public static float GetSquareMagnitude(this Vector3 vec3)
        {
            return vec3.X * vec3.X + vec3.Y * vec3.Y + vec3.Z * vec3.Z;
        }

        public static bool EpsEqual(this float flt, float otherFloat, float epsilon)
        {
            return otherFloat <= flt + epsilon && otherFloat >= flt - epsilon;
        }

        public static bool EpsGreaterThanOrEqual(this float flt, float otherFloat, float epsilon)
        {
            return flt > otherFloat || flt.EpsEqual(otherFloat, epsilon);
        }

        public static bool EpsLessThanOrEqual(this float flt, float otherFloat, float epsilon)
        {
            return flt < otherFloat || flt.EpsEqual(otherFloat, epsilon);
        }

        public static bool EpsEqual(this Vector3 vec3, Vector3 otherVec3, float epsilon)
        {
            return vec3.X.EpsEqual(otherVec3.X, epsilon) && vec3.Y.EpsEqual(otherVec3.Y, epsilon) && vec3.Z.EpsEqual(otherVec3.Z, epsilon);
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

        public static float[] AsArray(this Vector3 vec3)
        {
            return new float[] { vec3.X, vec3.Y, vec3.Z };
        }
    }
}
