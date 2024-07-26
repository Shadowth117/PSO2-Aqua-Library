using AquaModelLibrary.Data.FromSoft;
using AquaModelLibrary.Data.Utility;

namespace AquaModelLibrary.Core.ToolUX
{
    public class SMTSetting
    {
        public bool useMetaData = true;
        public bool applyMaterialNamesToMesh = false;
        public bool transformMesh = true;
        public bool extractUnreferencedMapData = true;
        public bool separateMSBDumpByModel = true;
        public bool doNotAdjustRootRotation = false;
        public SoulsGame soulsGame = SoulsGame.None;
        public ExportFormat exportFormat = ExportFormat.Fbx;
        public MirrorType mirrorType = MirrorType.Z;
        public CoordSystem coordSystem = CoordSystem.Max;
    }
}
