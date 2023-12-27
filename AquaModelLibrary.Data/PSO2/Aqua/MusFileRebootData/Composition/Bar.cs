using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData.Composition
{
    //Bar
    public class Bar
    {
        public List<Clip> mainClips = new List<Clip>();
        public List<Clip> subClips = new List<Clip>();
        public BarStruct barStruct;

        public Bar() { }

        public Bar(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            barStruct = sr.Read<BarStruct>();

            var bookmark = sr.Position;

            sr.Seek(offset + barStruct.mainClipOffset, SeekOrigin.Begin);
            for (int i = 0; i < barStruct.mainClipCount; i++)
            {
                mainClips.Add(new Clip(sr, offset));
            }

            sr.Seek(offset + barStruct.subClipOffset, SeekOrigin.Begin);
            for (int i = 0; i < barStruct.subClipCount; i++)
            {
                subClips.Add(new Clip(sr, offset));
            }

            sr.Seek(bookmark, SeekOrigin.Begin);
        }
    }

    public struct BarStruct
    {
        public int mainClipOffset;
        public int subClipOffset; //Usually nothing here
        public short beatsPerMinute;
        public short beat; //Sega's name for it
        public byte mainClipCount; //Flag for if main clip was used
        public byte subClipCount; //Flag for if sub clip was used
        public byte meVolume; //Sega's name for it
        public byte rhVolume; //Sega's name for it
    }
}
