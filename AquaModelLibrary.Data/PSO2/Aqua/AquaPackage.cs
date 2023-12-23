using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Data.PSO2.MiscPSO2Structs;
using AquaModelLibrary.Extensions.Readers;
using AquaModelLibrary.Helpers.Ice;
using System.IO;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    /// <summary>
    /// Contains either models and possibly texture patterns or motions. Consecutive files are seemingly only used for LOD outside of TPN. 
    /// TPN is extremely rare and likely got phased out early on.
    /// </summary>
    public class AquaPackage
    {
        /// <summary>
        /// We need the extension in case this isn't an AFP archive, in which case the extension is the most reasonable way to determine the file type.
        /// </summary>
        public string ext = null;
        public AFPMain afp = new AFPMain();
        public List<AFPBase> afpEnvelopes = new List<AFPBase>();
        public List<AquaObject> models = new List<AquaObject>();
        public List<TPNTexturePattern> tpns = new List<TPNTexturePattern>();

        public List<AquaMotion> anims = new List<AquaMotion>();

        public static readonly string[] fileExtensions = new string[]
        {
            "aqp\0",
            "trp\0",
            "aqm\0",
            "aqv\0",
            "aqw\0",
            "aqc\0",
            "trw\0",
            "trv\0",
            "trm\0",
        };

        public AquaPackage() { }

        public AquaPackage(byte[] file, string _ext)
        {
            Read(file, _ext);
        }

        public AquaPackage(BufferedStreamReaderBE<MemoryStream> sr, string _ext)
        {
            Read(sr, _ext);
        }

        public void Read(byte[] file, string _ext)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(sr, _ext);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr, string _ext)
        {
            ext = _ext;
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(sr.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            //Skip ice header junk if it's there
            IceMethods.SkipIceEnvelope(sr, fileExtensions, ref type, ref offset);

            //Deal with afp header or aqo. prefixing as needed
            if (type.Equals("afp\0"))
            {
                afp = sr.Read<AFPMain>();
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(sr.Peek<int>()));
                offset += 0x40;
            }
            //For deicer extracted aqo or tro. For w/e reason, they extract like this specially from that program. Nonstandard.
            else if (type.Equals("aqo\0") || type.Equals("tro\0"))
            {
                sr.Seek(0x4, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(sr.Peek<int>()));
                offset += 0x4;
            }

            //Workaround for older mods that made this in a ghetto way. The game still reads these so we still consider them valid
            //Used as well for if this is an afp-less file
            if (afp.fileCount == 0)
            {
                afp.fileCount = 1;
            }

            for(int i = 0; i < afp.fileCount; i++)
            {
                string currentExt;
                if (i > 0)
                {
                    sr.Seek(0x10, SeekOrigin.Current);
                }
                //If afp magic isn't 0, we have an AFPBase to read
                if (afp.magic != 0)
                {
                    afpEnvelopes.Add(sr.Read<AFPBase>());
                    offset = (int)sr.Position + 0x20;
                    currentExt = Path.GetExtension(afpEnvelopes[i].fileName.GetString());
                } else //Not an afp archive, so we use the original extension
                {
                    currentExt = ext;
                }

                type = Encoding.UTF8.GetString(BitConverter.GetBytes(sr.Peek<int>()));
               
                //Handle based on known
                switch(currentExt)
                {
                    case ".aqp":
                    case ".trp":
                    case ".aqo":
                    case ".tro":
                        switch(type)
                        {
                            case "NIFL":
                                models.Add();
                                break;
                            case "VTBF":
                                models.Add();
                                break;
                        }
                        break;
                    case ".aqm":
                    case ".aqv":
                    case ".aqw":
                    case ".aqc":
                    case ".trm":
                    case ".trv":
                    case ".trw":
                        switch (type)
                        {
                            case "NIFL":
                                anims.Add();
                                break;
                            case "VTBF":
                                anims.Add();
                                break;
                        }
                        break;
                    case ".tpn":
                        tpns.Add();
                        break;
                }
            }
        }
    }
}
