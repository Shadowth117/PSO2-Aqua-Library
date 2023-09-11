using static AquaModelLibrary.Extra.SoulsConvert;

namespace AquaModelLibrary.ToolUX
{
    public class SMTSetting
    {
        public bool useMetaData = true;
        public bool mirrorMesh = true;
        public bool applyMaterialNamesToMesh = false;
        public bool transformMesh = true;
        public bool extractUnreferencedMapData = true;
        public SoulsGame soulsGame = SoulsGame.None;
    }
}
