namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// These are placed directly after POF0 data. Not all ARCs have these.
    /// </summary>
    public class ARCFileRef
    {
        public int fileOffset;
        public int relativeNameOffset;

        public string name;
    }
}
