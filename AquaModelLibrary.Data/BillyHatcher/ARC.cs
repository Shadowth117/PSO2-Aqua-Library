using AquaModelLibrary.Data.BillyHatcher.ARCData;
using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher
{
    /// <summary>
    /// ARC is similar to something like PSO2 NIFL where it's more of a wrapper for other formats. .lnd also uses a variant of this at times.
    /// </summary>
    public class ARC
    {
        public ARCHeader arcHeader;
        public List<uint> pof0Offsets = new List<uint>();
        public List<ARCFileRef> group1FileReferences = new List<ARCFileRef>();
        public List<ARCFileRef> group2FileReferences = new List<ARCFileRef>();
        public List<string> group1FileNames = new List<string>();
        public List<string> group2FileNames = new List<string>();
        public List<byte[]> files = new List<byte[]>();
        public ARC() { }

        public ARC(byte[] file)
        {
            Read(file);
        }

        public ARC(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public virtual void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public virtual void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            //Generic ARC header
            arcHeader = new ARCHeader();
            arcHeader.fileSize = sr.ReadBE<int>();
            arcHeader.pof0Offset = sr.ReadBE<int>();
            arcHeader.pof0OffsetsSize = sr.ReadBE<int>();
            arcHeader.group1FileCount = sr.ReadBE<int>();

            arcHeader.group2FileCount = sr.ReadBE<int>();
            arcHeader.magic = sr.ReadBE<int>();
            arcHeader.unkInt0 = sr.ReadBE<int>();
            arcHeader.unkInt1 = sr.ReadBE<int>();

            //Get model references
            sr.Seek(0x20 + arcHeader.pof0Offset, SeekOrigin.Begin);
            pof0Offsets = POF0.GetRawPOF0Offsets(sr.ReadBytes(sr.Position, arcHeader.pof0OffsetsSize));
            sr.Seek(arcHeader.pof0OffsetsSize, SeekOrigin.Current);

            for (int i = 0; i < arcHeader.group1FileCount; i++)
            {
                ARCFileRef modelRef = new ARCFileRef();
                modelRef.fileOffset = sr.ReadBE<int>();
                modelRef.relativeNameOffset = sr.ReadBE<int>();
                group1FileReferences.Add(modelRef);
            }
            for (int i = 0; i < arcHeader.group2FileCount; i++)
            {
                ARCFileRef fileRef = new ARCFileRef();
                fileRef.fileOffset = sr.ReadBE<int>();
                fileRef.relativeNameOffset = sr.ReadBE<int>();
                group2FileReferences.Add(fileRef);
            }

            //Get file names
            var nameStart = sr.Position;
            foreach (var modelRef in group1FileReferences)
            {
                sr.Seek(nameStart + modelRef.relativeNameOffset, SeekOrigin.Begin);
                group1FileNames.Add(sr.ReadCString());
            }
            foreach (var modelRef in group2FileReferences)
            {
                sr.Seek(nameStart + modelRef.relativeNameOffset, SeekOrigin.Begin);
                group2FileNames.Add(sr.ReadCString());
            }
        }

        public static List<string> ReadTexNames(BufferedStreamReaderBE<MemoryStream> sr)
        {
            List<string> texnames = new List<string>();

            var arcRefTable = new ARCRefTableHead();
            arcRefTable.entryOffset = sr.ReadBE<int>();
            arcRefTable.entryCount = sr.ReadBE<int>();

            List<ARCRefEntry> refEntries = new List<ARCRefEntry>();
            sr.Seek(0x20 + arcRefTable.entryOffset, SeekOrigin.Begin);
            for (int i = 0; i < arcRefTable.entryCount; i++)
            {
                ARCRefEntry refEntry = new ARCRefEntry();
                refEntry.textOffset = sr.ReadBE<int>();
                refEntry.unkInt0 = sr.ReadBE<int>();
                refEntry.unkInt1 = sr.ReadBE<int>();
                refEntries.Add(refEntry);
            }
            foreach (ARCRefEntry entry in refEntries)
            {
                sr.Seek(entry.textOffset + 0x20, SeekOrigin.Begin);
                texnames.Add(sr.ReadCString());
            }

            return texnames;
        }
    }
}
