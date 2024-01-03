using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData
{
    //MO Header
    public struct MOHeader
    {
        public int variant; //0xE0, type 0x9 //Seems to be different pending type of anim. 0x10002 standard anims, 0x10012 player anims, 0x10004 camera, 0x20 material anims
        public int loopPoint; //0xE1, type 0x9 //Loop point? Only seen in live dance aqms and lower than anim range. Otherwise, 0.
        public int endFrame; //0xE2, type 0x9 //Last frame of the animation (0 based)
        public float frameSpeed; //0xE3, type 0xA //FPS of the animation. PSO2's default is 30.0f

        public int unkInt0;      //0xE4, type 0x9 //Always 0x4 for VTBF, always 0x2 for NIFL anims
        public int nodeCount;    //0xE5, type 0x9 //Number of animated nodes (effect type nodes not included) plus one for NodeTreeFlag stuff if a player anim.
        public int boneTableOffset; //NIFL only, always 0x50. Points to start of the bone name table in NIFL anims
        public PSO2String testString; //0xE6, type 0x2 //Always a string of just "test". Technically a standard 0x20 length PSO2 string, but always observed as "test"

        public int reserve0;

        public MOHeader(List<Dictionary<int, object>> moRaw)
        {
            variant = (int)moRaw[0][0xE0];
            loopPoint = (int)moRaw[0][0xE1];
            endFrame = (int)moRaw[0][0xE2];
            frameSpeed = (float)moRaw[0][0xE3];
            unkInt0 = (int)moRaw[0][0xE4];
            nodeCount = (int)moRaw[0][0xE5];
            testString = new PSO2String((byte[])moRaw[0][0xE6]);

            boneTableOffset = 0;
            reserve0 = 0;
        }

        public byte[] GetBytesVTBF(string motionType)
        {
            List<byte> outBytes = new List<byte>();

            VTBFMethods.AddBytes(outBytes, 0xE0, 0x9, BitConverter.GetBytes(variant));
            VTBFMethods.AddBytes(outBytes, 0xE1, 0x9, BitConverter.GetBytes(loopPoint));
            VTBFMethods.AddBytes(outBytes, 0xE2, 0x9, BitConverter.GetBytes(endFrame));
            VTBFMethods.AddBytes(outBytes, 0xE3, 0x9, BitConverter.GetBytes(frameSpeed));
            VTBFMethods.AddBytes(outBytes, 0xE4, 0x9, BitConverter.GetBytes(unkInt0));
            VTBFMethods.AddBytes(outBytes, 0xE5, 0x9, BitConverter.GetBytes(nodeCount));

            //Test String
            string testStr = testString.GetString();
            VTBFMethods.AddBytes(outBytes, 0xE6, 0x02, (byte)testStr.Length, Encoding.UTF8.GetBytes(testStr));

            VTBFMethods.WriteTagHeader(outBytes, motionType, (ushort)nodeCount, 0x7);

            return outBytes.ToArray();
        }

    }
}
