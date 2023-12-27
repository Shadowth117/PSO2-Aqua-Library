using AquaModelLibrary.Helpers.Readers;
using System.IO;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class CommandObject
    {
        public CommandStruct cmdStruct;
        public string type = null;
        public string text1 = null;
        public string text2 = null;
        public string text3 = null;

        public CommandObject() { }

        public CommandObject(int offset, long address, BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr.Seek(offset + address, SeekOrigin.Begin);
            cmdStruct = sr.Read<CommandStruct>();
            type = sr.ReadCStringValidOffset(cmdStruct.typePtr, offset);
            text1 = sr.ReadCStringValidOffset(cmdStruct.text1Ptr, offset);
            text2 = sr.ReadCStringValidOffset(cmdStruct.text2Ptr, offset);
            text3 = sr.ReadCStringValidOffset(cmdStruct.text3Ptr, offset);
        }
    }

    public struct CommandStruct
    {
        public int typePtr;
        public int text1Ptr;
        public int text2Ptr;
        public int text3Ptr;

        public int int_10;
        public int int_14;
    }
}
