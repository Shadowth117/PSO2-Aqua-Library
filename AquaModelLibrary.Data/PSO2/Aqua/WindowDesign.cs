using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    /// <summary>
    /// WindowDeSigN
    /// </summary>
    public class WindowDesign : AquaCommon
    {
        public RootNodeObject rootNode = null;
        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "wdsn"
            };
        }

        public WindowDesign() { }

        public WindowDesign(byte[] file) : base(file) { }

        public WindowDesign(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            sr.Seek(rel0.REL0DataStart + offset, SeekOrigin.Begin);
            sr.Seek(sr.Read<int>() + offset, SeekOrigin.Begin);
            rootNode = new RootNodeObject(sr, offset);
        }

        public struct RootNode
        {
            public byte bt_00;
            public byte EncodingType;
            public byte childCount;
            public byte bt_03;

            public int int_04;
            public int namePtr;
            public int stringPtr_0C;

            public int stringPtr_10;
            public int int_14;
            public int stringPtr_18;
            public int stringPtr_1C;

            public int stringPtr_20;
            public ushort usht_24; //Element dimension info?
            public ushort usht_26;
            public ushort usht_28;
            public ushort usht_2A;
            /// <summary>
            /// Usually -1
            /// </summary>
            public int int_2C;

            public ushort usht_30; //More dimension info?
            public ushort usht_32;
            public ushort usht_34;
            public ushort usht_36;
            public int iconPtrPtr;
            public int childNodePtrPtr;
            public int stringPtr_40;
        }

        public class RootNodeObject
        {
            public RootNode rootStruct;

            public string objName = null;
            public string str0C = null;
            public string str10 = null;
            public string str18 = null;
            public string str1C = null;
            public string str20 = null;

            public string str38 = null;
            public string str40 = null;

            public RootNodeObject() { }

            public RootNodeObject(BufferedStreamReaderBE<MemoryStream> sr, int offset) 
            {
                rootStruct = sr.Read<RootNode>();

                //var bookmark = sr.Position;

                objName = ReadWindowDesignString(sr, rootStruct.namePtr, offset, rootStruct.EncodingType);
                str0C = ReadWindowDesignString(sr, rootStruct.stringPtr_0C, offset, rootStruct.EncodingType);
                str10 = ReadWindowDesignString(sr, rootStruct.stringPtr_10, offset, rootStruct.EncodingType);
                str18 = ReadWindowDesignString(sr, rootStruct.stringPtr_18, offset, rootStruct.EncodingType);
                str1C = ReadWindowDesignString(sr, rootStruct.stringPtr_1C, offset, rootStruct.EncodingType);
                str20 = ReadWindowDesignString(sr, rootStruct.stringPtr_20, offset, rootStruct.EncodingType);
                sr.Seek(rootStruct.iconPtrPtr + offset, SeekOrigin.Begin);
                str38 = ReadWindowDesignString(sr, sr.Read<int>(), offset, rootStruct.EncodingType); 
                str40 = ReadWindowDesignString(sr, rootStruct.stringPtr_40, offset, rootStruct.EncodingType);

                //sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        protected static string ReadWindowDesignString(BufferedStreamReaderBE<MemoryStream> sr, int ptr, int offset, byte type)
        {
            if(ptr <= 0)
            {
                return "";
            }    
            switch(type)
            {
                case 1:
                    return sr.ReadUTF16String(ptr + offset, 0x100);
                case 8:
                case 9:
                    return sr.ReadUTF8String(ptr + offset, 0x100);
                default:
                    throw new Exception($"Unexpected type {type}!");
            }
        }

    }
}
