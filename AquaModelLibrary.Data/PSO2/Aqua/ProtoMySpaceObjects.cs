using AquaModelLibrary.Helpers.Readers;
using Reloaded.Memory.Streams;
using System.IO;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //Obsolete as of Ver2. Briefly found in the game files prior to Creative Space release.
    public class ProtoMySpaceObjectsSettings : AquaCommon
    {
        public List<MSOEntryObject> msoEntries = new List<MSOEntryObject>();
        public int entryOffset = -1;
        public int entryCount = -1;
        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "mso\0"
            };
        }
        public ProtoMySpaceObjectsSettings() { }

        public ProtoMySpaceObjectsSettings(byte[] file) : base(file) { }

        public ProtoMySpaceObjectsSettings(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            entryOffset = sr.Read<int>();
            entryCount = sr.Read<int>();

            //Entries
            sr.Seek(offset + entryOffset, SeekOrigin.Begin);
            for (int i = 0; i < entryCount; i++)
            {
                MSOEntryObject msoEntry = new MSOEntryObject();
                msoEntry.msoEntry = sr.Read<MSOEntry>();

                var bookmark = sr.Position;
                if (msoEntry.msoEntry.asciiNameOffset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.asciiNameOffset, SeekOrigin.Begin);
                    msoEntry.asciiName = sr.ReadCString();
                }
                else
                {
                    msoEntry.asciiName = "";
                }

                if (msoEntry.msoEntry.utf8DescriptorOffset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.utf8DescriptorOffset, SeekOrigin.Begin);
                    msoEntry.utf8Descriptor = sr.ReadUTF8String();
                }
                else
                {
                    msoEntry.utf8Descriptor = "";
                }

                if (msoEntry.msoEntry.asciiTrait1Offset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.asciiTrait1Offset, SeekOrigin.Begin);
                    msoEntry.asciiTrait1 = sr.ReadCString();
                }
                else
                {
                    msoEntry.asciiTrait1 = "";
                }

                if (msoEntry.msoEntry.asciiTrait2Offset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.asciiTrait2Offset, SeekOrigin.Begin);
                    msoEntry.asciiTrait2 = sr.ReadCString();
                }
                else
                {
                    msoEntry.asciiTrait2 = "";
                }

                if (msoEntry.msoEntry.asciiTrait3Offset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.asciiTrait3Offset, SeekOrigin.Begin);
                    msoEntry.asciiTrait3 = sr.ReadCString();
                }
                else
                {
                    msoEntry.asciiTrait3 = "";
                }

                if (msoEntry.msoEntry.asciiTrait4Offset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.asciiTrait4Offset, SeekOrigin.Begin);
                    msoEntry.asciiTrait4 = sr.ReadCString();
                }
                else
                {
                    msoEntry.asciiTrait4 = "";
                }

                if (msoEntry.msoEntry.groupNameOffset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.groupNameOffset, SeekOrigin.Begin);
                    msoEntry.groupName = sr.ReadCString();
                }
                else
                {
                    msoEntry.groupName = "";
                }

                if (msoEntry.msoEntry.asciiTrait5Offset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.asciiTrait5Offset, SeekOrigin.Begin);
                    msoEntry.asciiTrait5 = sr.ReadCString();
                }
                else
                {
                    msoEntry.asciiTrait5 = "";
                }

                msoEntries.Add(msoEntry);
                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public class MSOEntryObject
        {
            public MSOEntry msoEntry;

            public string asciiName = null;
            public string utf8Descriptor = null;
            public string asciiTrait1 = null;
            public string asciiTrait2 = null;
            public string asciiTrait3 = null;
            public string asciiTrait4 = null;
            public string groupName = null;
            public string asciiTrait5 = null;
        }

        public struct MSOEntry
        {
            public int asciiNameOffset;
            public int int_04;
            public int int_08;
            public int utf8DescriptorOffset;

            public int int_10;
            public int asciiTrait1Offset;
            public int int_18;
            public Vector3 vec3_1C;

            //0x28
            public int asciiTrait2Offset;
            public int asciiTrait3Offset;

            public int asciiTrait4Offset;
            public int groupNameOffset;
            public int int_38;
            public int asciiTrait5Offset;

            public ushort usht_40;
            public ushort usht_42;
            public ushort usht_44;
            public ushort usht_46;
        }
    }
}
