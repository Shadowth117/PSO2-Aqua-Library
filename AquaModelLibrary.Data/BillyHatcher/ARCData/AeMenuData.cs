using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public class AeMenuData
    {
        public ARCHeader header;
        public byte[] menuData = null;
        public NJTextureList texList = null;
        public PuyoFile gvm = null;
        public Dictionary<string, byte[]> gvps = new();
        public AeMenuData() { }

        public AeMenuData(byte[] file)
        {
            Read(file);
        }

        public AeMenuData(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }
        public void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            header = ARC.ReadArcHeader(sr);

            var menuOffset = sr.ReadBE<int>();
            var texListOffset = sr.ReadBE<int>();
            var menuSize = texListOffset - menuOffset;

            var gvmOffset = sr.ReadBE<int>();
            var gvpsOffset = sr.ReadBE<int>();

            //Menu Data
            menuData = sr.ReadBytes(menuOffset + 0x20, menuSize);

            //Texture list
            sr.Seek(0x20 + texListOffset, SeekOrigin.Begin);
            texList = new NJTextureList(sr, 0x20);

            //Texture Archive
            sr.Seek(0x20 + gvmOffset, SeekOrigin.Begin);
            gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));

            //GVP Data
            if(menuOffset != 0xC)
            {

                sr.Seek(0x20 + gvpsOffset, SeekOrigin.Begin);
                long bookmark = sr.Position;
                for (int i = 0; i < gvm.Entries.Count; i++)
                {
                    sr.Seek(bookmark, SeekOrigin.Begin);
                    if (((GVMEntry)gvm.Entries[i]).NeedsExternalPalette())
                    {
                        var offset = sr.ReadBE<int>();
                        bookmark = sr.Position;
                        sr.Seek(offset + 0x24, SeekOrigin.Begin);
                        var gvpSize = sr.Read<int>();
                        gvps.Add(gvm.Entries[i].Name, sr.ReadBytes(offset + 0x20, gvpSize + 8));
                    }

                }
            }
            
        }
    }
}
