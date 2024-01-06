using AquaModelLibrary.Data.PSO2.Aqua;
using System.Numerics;

namespace AquaModelLibrary.Data.Utility
{
    public class TransformUtil
    {
        public static void ApplyAxisMirroringZ(AquaNode aqn)
        {

        }

        public static Matrix4x4 ApplyAxisMirroringX(Matrix4x4 mat)
        {
            // mirror all base vectors at the local X axis
            mat.M11 = -mat.M11;
            mat.M21 = -mat.M21;
            mat.M31 = -mat.M31;
            mat.M41 = -mat.M41;

            // Now invert the X axis again to keep the matrix determinant positive.
            mat.M11 = -mat.M11;
            mat.M12 = -mat.M12;
            mat.M13 = -mat.M13;
            mat.M14 = -mat.M14;

            return mat;
        }

        public static Matrix4x4 ApplyAxisMirroringY(Matrix4x4 mat)
        {
            // mirror all base vectors at the local Y axis
            mat.M12 = -mat.M12;
            mat.M22 = -mat.M22;
            mat.M32 = -mat.M32;
            mat.M42 = -mat.M42;

            // Now invert the Y axis again to keep the matrix determinant positive.
            mat.M21 = -mat.M21;
            mat.M22 = -mat.M22;
            mat.M23 = -mat.M23;
            mat.M24 = -mat.M24;

            return mat;
        }

        public static Matrix4x4 ApplyAxisMirroringZ(Matrix4x4 mat)
        {
            // mirror all base vectors at the local Z axis
            mat.M13 = -mat.M13;
            mat.M23 = -mat.M23;
            mat.M33 = -mat.M33;
            mat.M43 = -mat.M43;

            // Now invert the Z axis again to keep the matrix determinant positive.
            mat.M31 = -mat.M31;
            mat.M32 = -mat.M32;
            mat.M33 = -mat.M33;
            mat.M34 = -mat.M34;

            return mat;
        }
    }
}
