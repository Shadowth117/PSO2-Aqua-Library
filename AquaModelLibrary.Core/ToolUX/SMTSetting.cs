using AquaModelLibrary.Data.FromSoft;

namespace AquaModelLibrary.Core.ToolUX
{
    public class SMTSetting
    {
        public bool useMetaData = true;
        public bool mirrorMesh = true;
        public bool applyMaterialNamesToMesh = false;
        public bool transformMesh = true;
        public bool extractUnreferencedMapData = true;
        public bool separateMSBDumpByModel = true;
        public bool addRootNodeLikeBlenderSmdImport = true;
        public bool doNotAdjustRootRotation = false;
        public SoulsGame soulsGame = SoulsGame.None;
    }
}
