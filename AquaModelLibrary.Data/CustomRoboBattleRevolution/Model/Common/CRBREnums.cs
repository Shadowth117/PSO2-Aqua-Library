namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public enum CRBRVertexType : int
    {
        /// <summary>
        /// No idea what the None here exists for, but it shows up sometimes
        /// </summary>
        None = 0x0,
        Position = 0x9,
        Normal = 0xA,
        Color = 0xB,
        UV1 = 0xD,
        UV2 = 0xE,
        End = 0xFF
    }
}
