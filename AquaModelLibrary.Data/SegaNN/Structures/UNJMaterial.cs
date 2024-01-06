using System.Numerics;

namespace  AquaModelLibrary.Data.NNStructs.Structures
{
    public struct UNJMaterialHeader
    {
        public byte diffuseTexCount;
        public byte effectTexCount;
        public short sht_02;
        public uint materialOffset;
    }

    public class UNJMaterial
    {
        public short disablingFlags;
        public short unkFlags;
        public int int_04;
        public int int_08;
        public Vector4 emissiveColor = new Vector4();
        public Vector4 ambientColor = new Vector4();
        public Vector4 diffuseColor = new Vector4();
        public Vector4 specularColor = new Vector4();
        public float specularValue;

        public List<UNJTextureProperties> propertyList = new List<UNJTextureProperties>();
        public List<byte[]> diffuseMapBytesRaw = new List<byte[]>();
        public List<string> diffuseMapTypes = new List<string>(); //Should be 1 of these for each diffuse tex
        public byte[] unknRange = new byte[0x18];
        public List<int> diffuseTexIds = new List<int>();
    }

    public struct UNJTextureProperties
    {
        public bool disableLighting; //0x21
        public bool alphaTestEnabled; //0x22
        public bool zTestEnabled; //0x23
        public bool stencilTestEnabled; //0x24
        public bool colorTestEnabled; //0x27
        public bool logicOpEnabled; //0x28
        public float scaleU; //0x48
        public float scaleV; //0x49
        public float offsetU; //0x4A
        public float offsetV; //0x4B
        public int diffuseEnabled; //0x5E - Kion set this to true if value < 2
        public bool clampU; //0xC7 -Encompasses ClampU and ClampV
        public bool clampV; //0xC7 -Encompasses ClampU and ClampV
        public int textureFunction; //0xC9 -Encompasses textureFunction and textureFunctionUsesAlpha
        public bool textureFunctionUsesAlpha; //0xC9 -Encompasses textureFunction and textureFunctionUsesAlpha
        public int alphaFunction; //0xDB -Encompasses alphaFunction and alphaRef
        public int alphaRef; //0xDB -Encompasses alphaFunction and alphaRef
        public int zTestFunction; //0xDE
        public int blendMode; //0xDF -Encompasses blendMode, blendFactorA, and blendFactorB
        public int blendFactorA; //0xDF -Encompasses blendMode, blendFactorA, and blendFactorB
        public int blendFactorB; //0xDF -Encompasses blendMode, blendFactorA, and blendFactorB
        public int blendFixedA; //0xE0
        public int blendFixedB; //0xE1
        public int logicOp; //0xE6
        public bool zWriteDisabled; //0xE7
        public int maskRGB; //0xE8
        public int maskAlpha; //0xE9
        //d == 0x0b means properties end 
    }
}
