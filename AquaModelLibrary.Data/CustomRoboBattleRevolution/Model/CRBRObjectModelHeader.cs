namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model
{
    public class CRBRObjectModelHeader : CRBRModelHeader
    {
        public int fileSize;
        public int offsetTable;

        public CRBRObjectModelHeader()
        {
            offset = 0x20;
        }
    }
}
