namespace AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData
{
    public class KeyData
    {
        public MSEG mseg = new MSEG();
        public List<MKEY> keyData = new List<MKEY>();
        //Typically, keyData is generally stored this way, but technically doesn't have to follow this convention:

        //Player/Standard animation data
        //Pos, Rot, Scale data

        //Camera animation Data - Unlike most animation data, only seems to ever contain one node. Seemingly just for fixed cameras. 
        //Pos, unk, unk, unk

        //Texture/UV anim data - Seems to contain many types of data, though somewhat untested
        //Seemingly 8 data sets. 

        //Node Tree Flag - Special subsection of data for player animations with an unknown purpose. Not necessary to include, but can be filled
        //with somewhat valid data if the user wishes
        //Pos, Rot data
        public MKEY GetMKEYofType(int type)
        {
            for (int i = 0; i < keyData.Count; i++)
            {
                if (keyData[i].keyType == type)
                {
                    return keyData[i];
                }
            }

            return null;
        }
    }
}
