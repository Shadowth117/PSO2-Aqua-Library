using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData.Composition
{
    //Clip
    public class Clip
    {
        public string clipFileName = null;
        public ClipStruct clipStruct;

        public Clip() { }

        public Clip(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            clipStruct = sr.Read<ClipStruct>();

            var bookmark = sr.Position;
            if (clipStruct.clipFileNameOffset != 0x10 && clipStruct.clipFileNameOffset != 0)
            {
                sr.Seek(offset + clipStruct.clipFileNameOffset, SeekOrigin.Begin);
                clipFileName = sr.ReadCString();
            }
            sr.Seek(bookmark, SeekOrigin.Begin);
        }
    }

    public struct ClipStruct
    {
        public int clipFileNameOffset;
        public byte volume; //? Typically 100
        public byte bt_6;  //These are probably boolean flags
        public byte bt_7;
        public byte bt_8;
    }
}
