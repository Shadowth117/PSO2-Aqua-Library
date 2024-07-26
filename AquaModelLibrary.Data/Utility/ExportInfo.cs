namespace AquaModelLibrary.Data.Utility
{
    public enum ExportFormat
    {
        Fbx = 0,
        Smd = 1,
    }

    /// <summary>
    /// For souls games, Z was always the default we used
    /// </summary>
    public enum MirrorType
    {
        None = 0,
        Z = 1,
        Y = 2,
        X = 3,
    }

    /// <summary>
    /// Based on FBX axis systems. 
    /// Note that Y-Up variants are ghetto and are treated as rotated 90 degrees on the X axis. 
    /// </summary>
    public enum CoordSystem
    {
        /// <summary>
        /// ROTATES MODEL 90 degrees on X
        /// Predefined axis system: OpenGL (UpVector = +Y, FrontVector = +Z, CoordSystem = +X (RightHanded))
        /// </summary>
        OpenGL = 0,
        /// <summary>
        /// Predefined axis system: Max (UpVector = +Z, FrontVector = -Y, CoordSystem = +X (RightHanded))
        /// </summary>
        Max = 1,
        /// <summary>
        /// ROTATES MODEL 90 degrees on X
        /// Predefined axis system: DirectX (UpVector = +Y, FrontVector = +Z, CoordSystem = -X (LeftHanded))
        /// </summary>
        DirectX = 2,
    }
}
