namespace AquaModelLibrary.Native.Fbx
{
    public static class FbxExporter
    {
        public static void ExportToFile(AquaObject aqo, AquaNode aqn, string destinationFilePath, bool includeMetadata) =>
            Native.FbxExporter.ExportToFile(aqo, aqn, destinationFilePath, includeMetadata);
    }
}
