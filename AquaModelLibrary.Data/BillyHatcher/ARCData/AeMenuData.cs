using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;
using System.Text;

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
                        if(offset < sr.BaseStream.Length) //Normally not a problem, but for the unused ae that recyles a gvp, it can cause issues...
                        {
                            sr.Seek(offset + 0x24, SeekOrigin.Begin);
                            var gvpSize = sr.Read<int>();
                            if(gvpSize < sr.BaseStream.Length)
                            {
                                gvps.Add(gvm.Entries[i].Name, sr.ReadBytes(offset + 0x20, gvpSize + 8));
                            }
                        }
                    }

                }
            }
            
        }

        public byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            List<int> pofSets = new List<int>();

            //AniModel
            pofSets.Add(0x0);
            pofSets.Add(0x4);
            pofSets.Add(0x8);
            
            if (gvps.Count > 0)
            {
                pofSets.Add(0xC);
            }

            outBytes.ReserveInt("aeOffset");
            outBytes.ReserveInt("texListOffset");
            outBytes.ReserveInt("gvmOffset");
            if(gvps.Count > 0)
            {
                outBytes.ReserveInt("gvpListOffset");
            }

            outBytes.FillInt("aeOffset", outBytes.Count);
            outBytes.AddRange(menuData);

            outBytes.FillInt("texListOffset", outBytes.Count);
            texList.Write(outBytes, pofSets);

            outBytes.AlignWriter(0x20);
            outBytes.FillInt("gvmOffset", outBytes.Count);
            outBytes.AddRange(gvm.GetBytes());

            if (gvps.Count > 0)
            {
                outBytes.FillInt("gvpListOffset", outBytes.Count);
                List<string> gvpOrder = new List<string>();
                for(int i = 0; i < gvm.Entries.Count; i++)
                {
                    var name = Path.GetFileNameWithoutExtension(gvm.Entries[i].Name);
                    if (gvps.ContainsKey(name))
                    {
                        gvpOrder.Add(name);
                        pofSets.Add(outBytes.Count);
                        outBytes.ReserveInt($"gvp{gvpOrder.Count - 1}");
                    }
                }
                for(int i = 0; i < gvpOrder.Count; i++)
                {
                    outBytes.FillInt($"gvp{i}", outBytes.Count);
                    outBytes.AddRange(gvps[gvpOrder[i]]);
                }

            }

            //Add POF0, insert header
            outBytes.AlignWriter(0x4);
            var pof0Offset = outBytes.Count;
            pofSets.Sort();
            var pof0 = POF0.GenerateRawPOF0(pofSets, true);
            outBytes.AddRange(pof0);

            var arcBytes = new List<byte>();
            arcBytes.AddValue(outBytes.Count + 0x20);
            arcBytes.AddValue(pof0Offset);
            arcBytes.AddValue(pof0.Length);
            arcBytes.AddValue(0);

            arcBytes.AddValue(0);
            arcBytes.Add(0x30);
            arcBytes.Add(0x31);
            arcBytes.Add(0x30);
            arcBytes.Add(0x30);
            arcBytes.AddValue(0);
            arcBytes.AddValue(0);

            outBytes.InsertRange(0, arcBytes);

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }
    }
}
