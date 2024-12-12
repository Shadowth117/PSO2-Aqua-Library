namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public class HelperFunctions
    {

        public static int GetCount(int offset, List<int> queue)
        {
            if (offset == 0)
            {
                return 0;
            }
            int finalOffset = 0;
            for (int i = 0; i < queue.Count; i++)
            {
                if (queue[i] != 0)
                {
                    finalOffset = (queue[i] - offset) / 4;
                    break;
                }
            }

            return finalOffset;
        }
    }
}
