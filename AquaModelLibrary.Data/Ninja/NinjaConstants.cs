namespace AquaModelLibrary.Data.Ninja
{
    public class NinjaConstants
    {
        public const double FromBAMSvalueToRadians = ((2 * Math.PI) / 65536.0);
        public const double ToBAMSValueFromRadians = (65536.0 / (2 * Math.PI));
    }

    public enum AlphaInstruction : int
    {
        Zero = 0,
        One = 1,
        OtherColor = 2,
        InverseOtherColor = 3,
        SourceAlpha = 4,
        InverseSourceAlpha = 5,
        DestinationAlpha = 6,
        InverseDestinationAlpha = 7,
    }
}
