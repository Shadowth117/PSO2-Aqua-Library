namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// These are placed directly after POF0 data. Not all ARCs have these.
    /// </summary>
    public struct ARCFileRef
    {
        public int fileOffset;
        public int relativeNameOffset;
    }
}
