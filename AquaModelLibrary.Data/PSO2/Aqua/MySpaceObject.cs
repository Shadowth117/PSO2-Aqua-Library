using AquaModelLibrary.Helpers.Readers;
using System.IO;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class MySpaceObjectsSettings : AquaCommon //Yes really
    {
        public List<MSOEntryObject> msoEntries = new List<MSOEntryObject>();
        public List<MSOCategoryObject> msoGroups = new List<MSOCategoryObject>();
        public List<MSOCategoryObject> msoTypes = new List<MSOCategoryObject>();
        public MSOHeader msoHeader;

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "mso\0"
            };
        }

        public MySpaceObjectsSettings() { }

        public MySpaceObjectsSettings(byte[] file) : base(file) { }

        public MySpaceObjectsSettings(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            msoHeader = sr.Read<MSOHeader>();

            //Groups
            sr.Seek(offset + msoHeader.groupOffset, SeekOrigin.Begin);
            for (int i = 0; i < msoHeader.groupCount; i++)
            {
                MSOCategoryObject msoPair = new MSOCategoryObject();
                msoPair.pair = sr.Read<MSOAddressPair>();

                var bookmark = sr.Position;
                sr.Seek(offset + msoPair.pair.offset, SeekOrigin.Begin);
                msoPair.name = sr.ReadCString();
                sr.Seek(bookmark, SeekOrigin.Begin);

                msoGroups.Add(msoPair);
            }

            //Types
            sr.Seek(offset + msoHeader.typeOffset, SeekOrigin.Begin);
            for (int i = 0; i < msoHeader.typeCount; i++)
            {
                MSOCategoryObject msoPair = new MSOCategoryObject();
                msoPair.pair = sr.Read<MSOAddressPair>();

                var bookmark = sr.Position;
                sr.Seek(offset + msoPair.pair.offset, SeekOrigin.Begin);
                msoPair.name = sr.ReadCString();
                sr.Seek(bookmark, SeekOrigin.Begin);

                msoTypes.Add(msoPair);
            }

            //Entries
            sr.Seek(offset + msoHeader.entryOffset, SeekOrigin.Begin);
            for (int i = 0; i < msoHeader.entryCount; i++)
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

                if (msoEntry.msoEntry.asciiTrait5Offset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.asciiTrait5Offset, SeekOrigin.Begin);
                    msoEntry.asciiTrait5 = sr.ReadCString();
                }
                else
                {
                    msoEntry.asciiTrait5 = "";
                }

                if (msoEntry.msoEntry.asciiTrait6Offset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.asciiTrait6Offset, SeekOrigin.Begin);
                    msoEntry.asciiTrait6 = sr.ReadCString();
                }
                else
                {
                    msoEntry.asciiTrait6 = "";
                }

                if (msoEntry.msoEntry.asciiTrait7Offset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.asciiTrait7Offset, SeekOrigin.Begin);
                    msoEntry.asciiTrait7 = sr.ReadCString();
                }
                else
                {
                    msoEntry.asciiTrait7 = "";
                }

                if (msoEntry.msoEntry.asciiTrait8Offset > 0x10)
                {
                    sr.Seek(offset + msoEntry.msoEntry.asciiTrait8Offset, SeekOrigin.Begin);
                    msoEntry.asciiTrait8 = sr.ReadCString();
                }
                else
                {
                    msoEntry.asciiTrait8 = "";
                }

                msoEntries.Add(msoEntry);
                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct MSOHeader
        {
            public int entryOffset;
            public int entryCount;
            public int groupOffset;
            public int groupCount;

            public int typeOffset;
            public int typeCount;
        }

        public class MSOEntryObject
        {
            public MSOEntry msoEntry;

            public string asciiName = null;
            public string asciiTrait1 = null;
            public string asciiTrait2 = null;
            public string asciiTrait3 = null;
            public string asciiTrait4 = null;
            public string asciiTrait5 = null;
            public string asciiTrait6 = null;
            public string asciiTrait7 = null;
            public string asciiTrait8 = null;
        }

        public struct MSOEntry
        {
            public int asciiNameOffset;
            public int gameVersion;          //A guess, reminiscent of NGS versioning
            public int asciiTrait1Offset;
            public int asciiTrait2Offset;

            public int itemId;               //A guess based on how other item ids look. Really dunno here
            public Vector3 vec3_14;

            public int asciiTrait3Offset;
            public int asciiTrait4Offset;
            public int asciiTrait5Offset;
            public int int_2C;

            public int asciiTrait6Offset;
            public int asciiTrait7Offset;
            public int int_38;
            public int asciiTrait8Offset;

            public byte bt_40;
            public byte bt_41;
            public byte bt_42;
            public byte bt_43;

            public int reserve_44;
        }

        public class MSOCategoryObject
        {
            public MSOAddressPair pair;
            public string name = "";
        }

        public struct MSOAddressPair
        {
            public int offset;
            public int count;
        }
    }
}
