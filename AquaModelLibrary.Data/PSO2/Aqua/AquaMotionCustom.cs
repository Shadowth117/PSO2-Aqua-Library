namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public partial class AquaMotion : AquaCommon
    {
        public class MorphKeyData
        {
            public string channelName;
            public List<float> weightKeyFrames = new();
            public List<float> weightTimes = new();
        }

        public List<List<MorphKeyData>> morphKeyDataListList = new();
    }
}
