namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    public enum VertFlags : int
    {
        VertPosition = 0x0,
        VertWeight = 0x1,
        VertNormal = 0x2,
        VertColor = 0x3,
        VertColor2 = 0x4,
        VertWeightIndex = 0xb,
        VertUV1 = 0x10,
        VertUV2 = 0x11,
        VertUV3 = 0x12,
        VertUV4 = 0x13,
        VertTangent = 0x20,
        VertBinormal = 0x21,
        Vert0x22 = 0x22,
        Vert0x23 = 0x23,
        Vert0x24 = 0x24,
        Vert0x25 = 0x25,
    }
    //Credit to MiscellaneousModder for this list:
    public enum baseWearIds : int
    {
        costume = 0x0,
        breastNeck = 0x1,
        front = 0x2,
        accessory1 = 0x3,
        back = 0x4,
        shoulder = 0x5,
        foreArm = 0x6,
        leg = 0x7,
        accessory2 = 0x8,
        headOrnament = 0x9,
        castBodyOrnament = 0xA,
        castLegsOrnament = 0xB,
        castArmsOrnament = 0xC,
        outerOrnament = 0xD,
    }

    public static class AQOConstants
    {
        public static readonly List<string> DefaultShaderNames = new List<string>() { "0398p", "0398" };
    }
}
