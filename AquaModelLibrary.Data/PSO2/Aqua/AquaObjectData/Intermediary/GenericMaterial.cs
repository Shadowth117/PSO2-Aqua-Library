using AquaModelLibrary.Helpers.MathHelpers;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary
{
    public class GenericMaterial
    {
        public List<string> texNames = null;
        public List<int> texUVSets = null;
        public List<string> shaderNames = null;
        public string blendType = null;
        public string specialType = null;
        public string matName = null;
        public int twoSided = 0; //0 False, 1 True. Higher values give unknown results
        public int alphaCutoff = 0; //0-255, defines when an alhpa value should cull out a pixel entirely
        public int srcAlpha = 5;
        public int destAlpha = 6;

        public Vector4 diffuseRGBA = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        public Vector4 unkRGBA0 = new Vector4(.9f, .9f, .9f, 1.0f);
        public Vector4 _sRGBA = new Vector4(0f, 0f, 0f, 1.0f);
        public Vector4 unkRGBA1 = new Vector4(0f, 0f, 0f, 1.0f);

        public int reserve0 = 0;
        public float unkFloat0 = 8;
        public float unkFloat1 = 1;
        public int unkInt0 = 100;
        public int unkInt1 = 0;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object o)
        {
            return Equals((GenericMaterial)o);
        }

        public bool Equals(GenericMaterial c)
        {

            // Optimization for a common success case.
            if (ReferenceEquals(this, c))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (GetType() != c.GetType())
            {
                return false;
            }

            //texNames
            if (texNames == null || c.texNames == null)
            {
                if (texNames != c.texNames)
                {
                    return false;
                }
            }
            else if (texNames.Count == c.texNames.Count)
            {
                for (int i = 0; i < texNames.Count; i++)
                {
                    if (texNames[i] != c.texNames[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            //texUvSets
            if (texUVSets == null || c.texUVSets == null)
            {
                if (texUVSets != c.texUVSets)
                {
                    return false;
                }
            }
            else if (texUVSets.Count == c.texUVSets.Count)
            {
                for (int i = 0; i < texUVSets.Count; i++)
                {
                    if (texUVSets[i] != c.texUVSets[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            //shaderNames
            if (shaderNames == null || c.shaderNames == null)
            {
                if (shaderNames != c.shaderNames)
                {
                    return false;
                }
            }
            else if (shaderNames.Count == c.shaderNames.Count)
            {
                for (int i = 0; i < shaderNames.Count; i++)
                {
                    if (shaderNames[i] != c.shaderNames[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return reserve0 == c.reserve0 && unkFloat0 == c.unkFloat0 && unkFloat1 == c.unkFloat1 && unkInt0 == c.unkInt0 && unkInt1 == c.unkInt1
                && blendType == c.blendType && MathExtras.isEqualVec4(diffuseRGBA, c.diffuseRGBA) && MathExtras.isEqualVec4(unkRGBA0, c.unkRGBA0)
                && specialType == c.specialType && twoSided == c.twoSided && MathExtras.isEqualVec4(_sRGBA, c._sRGBA) && MathExtras.isEqualVec4(unkRGBA1, c.unkRGBA1);
        }

        public static bool operator ==(GenericMaterial lhs, GenericMaterial rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(GenericMaterial lhs, GenericMaterial rhs) => !(lhs == rhs);

        public GenericMaterial Clone()
        {
            GenericMaterial genMat = new GenericMaterial();
            genMat.texNames = new List<string>(texNames);
            if (texUVSets != null)
            {
                genMat.texUVSets = new List<int>(texUVSets);
            }
            if (shaderNames != null)
            {
                genMat.shaderNames = new List<string>(shaderNames);
            }
            genMat.blendType = blendType;
            genMat.specialType = specialType;
            genMat.matName = matName;
            genMat.twoSided = twoSided;

            genMat.diffuseRGBA = diffuseRGBA;
            genMat.unkRGBA0 = unkRGBA0;
            genMat._sRGBA = _sRGBA;
            genMat.unkRGBA1 = unkRGBA1;

            genMat.reserve0 = reserve0;
            genMat.unkFloat0 = unkFloat0;
            genMat.unkFloat1 = unkFloat1;
            genMat.unkInt0 = unkInt0;
            genMat.unkInt1 = unkInt1;

            return genMat;
        }
    }

}
