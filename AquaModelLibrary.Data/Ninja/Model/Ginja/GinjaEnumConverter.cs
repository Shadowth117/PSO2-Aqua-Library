using AquaModelLibrary.Data.Gamecube;

namespace AquaModelLibrary.Data.Ninja.Model.Ginja
{
    /// <summary>
    /// Used to convert between NJ and GC enums
    /// </summary>
    public static class GCEnumConverter
    {
        public static AlphaInstruction GXToNJAlphaInstruction(GCBlendModeControl gx)
        {
            switch (gx)
            {
                case GCBlendModeControl.SrcAlpha:
                    return AlphaInstruction.SourceAlpha;
                case GCBlendModeControl.DstAlpha:
                    return AlphaInstruction.DestinationAlpha;
                case GCBlendModeControl.InverseSrcAlpha:
                    return AlphaInstruction.InverseSourceAlpha;
                case GCBlendModeControl.InverseDstAlpha:
                    return AlphaInstruction.InverseDestinationAlpha;
                case GCBlendModeControl.SrcColor:
                    return AlphaInstruction.OtherColor;
                case GCBlendModeControl.InverseSrcColor:
                    return AlphaInstruction.InverseOtherColor;
                case GCBlendModeControl.One:
                    return AlphaInstruction.One;
                case GCBlendModeControl.Zero:
                    return AlphaInstruction.Zero;
            }

            return AlphaInstruction.Zero;
        }

        public static GCBlendModeControl NJtoGXBlendModeControl(AlphaInstruction nj)
        {
            switch (nj)
            {
                case AlphaInstruction.SourceAlpha:
                    return GCBlendModeControl.SrcAlpha;
                case AlphaInstruction.DestinationAlpha:
                    return GCBlendModeControl.DstAlpha;
                case AlphaInstruction.InverseSourceAlpha:
                    return GCBlendModeControl.InverseSrcAlpha;
                case AlphaInstruction.InverseDestinationAlpha:
                    return GCBlendModeControl.InverseDstAlpha;
                case AlphaInstruction.OtherColor:
                    return GCBlendModeControl.SrcColor;
                case AlphaInstruction.InverseOtherColor:
                    return GCBlendModeControl.InverseSrcColor;
                case AlphaInstruction.One:
                    return GCBlendModeControl.One;
                case AlphaInstruction.Zero:
                    return GCBlendModeControl.Zero;
            }

            return GCBlendModeControl.Zero;
        }

    }
}
