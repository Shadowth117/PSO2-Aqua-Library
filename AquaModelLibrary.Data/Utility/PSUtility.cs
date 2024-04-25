namespace AquaModelLibrary.Data.Utility
{
    public class PSUtility
    {
        public static string GetPSRootPath(string currentPath)
        {
            int i = 0;
            while (currentPath != null && !File.Exists(Path.Combine(currentPath, "eboot.bin")))
            {
                currentPath = Path.GetDirectoryName(currentPath);
                i++;
                //Should seriously never ever ever happen, but screw it
                if (i == 255)
                {
                    break;
                }
            }

            return currentPath;
        }
    }
}
