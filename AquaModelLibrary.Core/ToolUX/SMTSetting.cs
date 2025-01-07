using AquaModelLibrary.Data.FromSoft;
using AquaModelLibrary.Data.Utility;

namespace AquaModelLibrary.Core.ToolUX
{
    public class SMTSetting
    {
        public bool useMetaData { get; set; } = true;
        public bool applyMaterialNamesToMesh { get; set; } = false;
        public bool transformMesh { get; set; } = true;
        public bool extractUnreferencedMapData { get; set; } = true;
        public bool separateMSBDumpByModel { get; set; } = true;
        public bool addFBXRootNode { get; set; } = false;
        public bool addFlverDummies { get; set; } = false;
        public bool parentDummiesToAttachNodes { get; set; } = true;
        public SoulsGame soulsGame { get; set; } = SoulsGame.None;
        public ExportFormat exportFormat { get; set; } = ExportFormat.Fbx;
        public MirrorType mirrorType { get; set; } = MirrorType.Z;
        public CoordSystem coordSystem { get; set; } = CoordSystem.Max;
    }
}
