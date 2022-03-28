namespace AquaModelLibrary.Native.Fbx.Interfaces
{
    public interface IFbxExporter
    {
        void ExportToFile(AquaObject aqo, AquaNode aqn, string destinationFilePath);
    }
}
